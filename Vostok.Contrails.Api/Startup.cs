using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Vostok.Contrails.Client;
using Vostok.Logging;
using Vostok.Logging.Serilog;

namespace Vostok.Contrails.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            services.AddSingleton<IContrailsClient>(x =>
            {
                var configuration = x.GetService<IOptions<ContrailsClientSettings>>();
                return new ContrailsClient(configuration.Value, log);
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
        }
    }
}
