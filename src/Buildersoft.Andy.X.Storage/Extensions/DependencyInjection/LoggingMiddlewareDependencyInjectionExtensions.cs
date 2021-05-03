using Buildersoft.Andy.X.Storage.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Buildersoft.Andy.X.Storage.Extensions.DependencyInjection
{
    public static class LoggingMiddlewareDependencyInjectionExtensions
    {
        public static IApplicationBuilder UseHttpReqResLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoggingMiddleware>();
        }

        public static void AddSerilogLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}
