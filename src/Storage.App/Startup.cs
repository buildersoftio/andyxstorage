using Buildersoft.Andy.X.Storage.App.Extensions.DependencyInjection;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace Buildersoft.Andy.X.Storage.App
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
            services.AddEntityFrameworkSqlite().AddDbContext<TenantContext>();
            // Load configuration
            services.AddConfigurations(Configuration);
            services.AddSerilogLoggingConfiguration(Configuration);
            services.AddNodeServiceRepository();
            services.AddIOServices();
            services.AddStartService();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerfactory, IServiceProvider provider)
        {
            loggerfactory.AddSerilog();
            app.StartServices(provider);
        }
    }
}
