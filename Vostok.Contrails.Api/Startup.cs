using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Vostok.Commons.Extensions.UnitConvertions;
using Vostok.Contrails.Client;
using Vostok.Hosting;
using Vostok.Instrumentation.AspNetCore;
using Vostok.Logging;
using Vostok.Logging.Logs;
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
	        services.AddSingleton(x => x.GetRequiredService<IVostokHostingEnvironment>().Log ?? new SilentLog());
			services.Configure<ContrailsClientSettings>(options => Configuration.GetSection("ContrailsClient").Bind(options));
            services.AddMvc()
                .AddJsonOptions(
                    opt =>
                    {
                        opt.SerializerSettings.Converters.Add(new JsonGuidConverter());
                    });
            services.AddSingleton<Func<IContrailsClient>>(x =>
            {
                var log = x.GetService<ILog>();
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
                    var metricScope = rootScope.WithTag(MetricsTagNames.Type,"api");
                    return new MetricContainer
                    {
                        SuccessCounter = metricScope.WithTag("status","200").Counter(FlushMetricsInterval, "requests"),
                        ErrorCounter = metricScope.WithTag("status", "500").Counter(FlushMetricsInterval, "requests")
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILog log)
        {
            if (env.EnvironmentName.Equals("dev", StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
            //app.UseVostokLogging().UseVostokSystemMetrics(FlushMetricsInterval);
            log.Info("Configured app");
        }

        private TimeSpan FlushMetricsInterval
        {
            get
            {
                var flushMetricsInterval = Configuration.GetValue<TimeSpan>("FlushMetricsInterval");
                if (flushMetricsInterval == TimeSpan.Zero)
                    flushMetricsInterval = 1.Minutes();
                return flushMetricsInterval;
            }
        }
    }
}
