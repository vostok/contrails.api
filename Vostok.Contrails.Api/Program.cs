using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Vostok.Hosting;
using Vostok.Instrumentation.AspNetCore;
using Vostok.Logging;
using Vostok.Logging.Serilog;

namespace Vostok.Contrails.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildVostokHost(args).Run();
        }


        private static IVostokHost BuildVostokHost(params string[] args)
        {
            return new VostokHostBuilder<Application>()
                .SetServiceInfo("vostok", "frontier")
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);
                    configurationBuilder.AddEnvironmentVariables();
                    configurationBuilder.AddJsonFile("appsettings.json");
                })
                .ConfigureHost((context, hostConfigurator) =>
                {
                    var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug();
                    if (context.Configuration.GetSection("hostLog").GetValue<bool>("console"))
                    {
                        loggerConfiguration = loggerConfiguration
                            .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level:u3} [{Thread}] {Message:l}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Information);
                    }
                    var pathFormat = context.Configuration.GetSection("hostLog")["pathFormat"];
                    if (!string.IsNullOrEmpty(pathFormat))
                    {
                        loggerConfiguration = loggerConfiguration
                            .WriteTo.RollingFile(pathFormat, outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level:u3} [{Thread}] {Message:l}{NewLine}{Exception}");
                    }
                    var hostLog = new SerilogLog(loggerConfiguration.CreateLogger());
                    hostConfigurator.SetHostLog(hostLog);
                })
                .ConfigureAirlock((context, configurator) =>
                {
                    configurator.SetLog(context.HostingEnvironment.Log.FilterByLevel(LogLevel.Error));
                })
                .Build();
        }

        //private static IWebHost BuildWebHost(string[] args)
        //{
        //    return WebHost.CreateDefaultBuilder(args)
        //        .UseKestrel()
        //        .ConfigureAppConfiguration((hostingContext, config) =>
        //        {
        //            config.AddJsonFile("appsettings.json", false, true);
        //        })
        //        .UseUrls("http://+:6302/")
        //        .ConfigureAirlock()
        //        .ConfigureVostokMetrics()
        //        .ConfigureVostokLogging()
        //        .UseStartup<Startup>()
        //        .Build();
        //}
    }
}
