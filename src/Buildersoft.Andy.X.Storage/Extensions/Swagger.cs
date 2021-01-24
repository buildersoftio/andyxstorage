using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Extensions
{
    public static class Swagger
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1.1",
                    Title = "Buildersoft Andy X Data Storage",
                    Description = "Andy X Data Storage is an open-source standalone service that is used to store messages for Andy X. The Data Storage service is offers support for Multitenancy storage. X Data Storage hosts all messages and makes sure that all of them are readable for the client."
                });
            });
            return services;
        }
        public static IApplicationBuilder UseSwaggerView(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BuilderSoft Andy X Storage");
                c.DocumentTitle = "Buildersoft Andy X Storage";
                c.ShowExtensions();
                c.RoutePrefix = string.Empty;
            });
            return app;
        }
    }

}
