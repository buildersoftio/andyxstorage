using System;
using System.Text.Json.Serialization;
using Buildersoft.Andy.X.Storage.Extensions.DependencyInjection;
using Buildersoft.Andy.X.Storage.Logic.Services;
using Buildersoft.Andy.X.Storage.Providers;
using Buildersoft.Andy.X.Storage.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Buildersoft.Andy.X.Storage
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
            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddSerilogLoggingConfiguration(Configuration);

            // Add Health Checks
            services.AddHealthChecks();

            // Add Swagger UI for Endpoints Documentation
            services.AddSwagger();

            // Add HttpClientHandler
            services.AddHttpClientHandler();

            // Add Rest Services
            services.AddSingleton<ConnectionService>();

            // Add SignalR Connection Provider
            services.AddSingleton<HubConnectionProvider>();

            // Add SignalR DataStorage Client Service
            services.AddSingleton<NodeDataStorageService>();

            services.AddSignalRRepositories();
            services.AddSignalRServices();
            services.AddSignalREventHandlers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, ILoggerFactory loggerfactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerView();
            }

            app.InitializeSignalREventHandlers(serviceProvider);

            app.UseHttpReqResLogging();

            loggerfactory.AddSerilog();

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Mapping health checks
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
            });
        }
    }
}
