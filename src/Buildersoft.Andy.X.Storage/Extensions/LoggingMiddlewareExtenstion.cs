using Buildersoft.Andy.X.Storage.Middleware;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Extensions
{
    public static class LoggingMiddlewareExtenstion
    {
        public static IApplicationBuilder UseHttpReqResLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoggingMiddleware>();
        }
    }
}
