using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Vostok.Commons.Extensions.UnitConvertions;
using Vostok.Contrails.Client;
using Vostok.Instrumentation.AspNetCore;
using Vostok.Logging;
using Vostok.Logging.Serilog;
using Vostok.Metrics;
using Vostok.Metrics.Meters;

namespace Vostok.Contrails.Api
{
    public class MetricContainer
    {
        public ICounter SuccessCounter { get; set; }
        public ICounter ErrorCounter { get; set; }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private IContrailsClient contrailsClient;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var log = new SerilogLog(Log.Logger)
                .WithFlowContext();
            services.Configure<ContrailsClientSettings>(options => Configuration.GetSection("ContrailsClient").Bind(options));
            services.AddMvc()
                .AddJsonOptions(
                    opt =>
                    {
                        opt.SerializerSettings.Converters.Add(new JsonGuidConverter());
                    });
            services.AddSingleton<Func<IContrailsClient>>(x =>
            {
                var configuration = x.GetService<IOptions<ContrailsClientSettings>>();
                return () =>
                {
                    if (contrailsClient != null)
                        return contrailsClient;
                    lock (this)
                    {
                        contrailsClient = new ContrailsClient(configuration.Value, log);
                    }
                    return contrailsClient;
                };
            });
            services.AddSingleton(
                x =>
                {
                    var rootScope = x.GetService<IMetricScope>();
                    var metricScope = rootScope.WithTag("type","api");
                    return new MetricContainer
                    {
                        SuccessCounter = metricScope.WithTag("status","200").Counter(10.Seconds(), "requests"),
                        ErrorCounter = metricScope.WithTag("status", "500").Counter(10.Seconds(), "requests")
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.EnvironmentName.Equals("dev", StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
            app.UseVostokLogging().UseVostokSystemMetrics(TimeSpan.FromSeconds(10));
        }
    }
}
