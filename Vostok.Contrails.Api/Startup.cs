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
        public ICounter RequestCounter { get; set; }
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
                x => new MetricContainer
                {
                    RequestCounter = x.GetService<IMetricScope>().Counter(10.Seconds(), "requests"),
                    ErrorCounter = x.GetService<IMetricScope>().Counter(10.Seconds(), "errors")
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
            app.UseVostokLogging().UseVostokSystemMetrics(TimeSpan.FromSeconds(10));
        }
    }
}
