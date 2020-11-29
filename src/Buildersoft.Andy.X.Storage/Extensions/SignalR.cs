using Buildersoft.Andy.X.Storage.Data.Model;
using Buildersoft.Andy.X.Storage.Data.Model.Products;
using Buildersoft.Andy.X.Storage.FileConfig.Configurations;
using Buildersoft.Andy.X.Storage.Logic.Repositories;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using Buildersoft.Andy.X.Storage.Logic.Services;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Buildersoft.Andy.X.Storage.Providers;
using Buildersoft.Andy.X.Storage.Services.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Extensions
{
    public static class SignalR
    {
        public static IServiceCollection AddSignalREventHandlers(this IServiceCollection services)
        {
            services.AddSingleton<StorageEventHandler>();
            services.AddSingleton<TenantEventHandler>();
            services.AddSingleton<ProductEventHandler>();
            services.AddSingleton<ComponentEventHandler>();
            services.AddSingleton<BookEventHandler>();
            services.AddSingleton<MessageEventHandler>();
            services.AddSingleton<ReaderEventHandler>();

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSingleton<ITenantService, TenantService>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<IComponentService, ComponentService>();
            services.AddSingleton<IBookService, BookService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IReaderService, ReaderService>();

            return services;
        }
        public static IServiceCollection AddSignalRRepositories(this IServiceCollection services)
        {
            services.AddSingleton<ITenantRepository, TenantRepository>();
            return services;
        }

        public static IApplicationBuilder InitializeSignalREventHandlers(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>()
             .CreateLogger<HubConnectionProvider>();

            if (ConfigFile.GetDataStorageSettings().DataStorageName != "" && ConfigFile.GetAndyXSettings().Name != "")
            {
                var storageEventHandler = serviceProvider.GetService<StorageEventHandler>();
                var tenantEventHandler = serviceProvider.GetService<TenantEventHandler>();
                var productEventHandler = serviceProvider.GetService<ProductEventHandler>();
                var componentEventHandler = serviceProvider.GetService<ComponentEventHandler>();
                var bookEventHandler = serviceProvider.GetService<BookEventHandler>();
                var messageEventHandler = serviceProvider.GetService<MessageEventHandler>();
                var readerEventHandler = serviceProvider.GetService<ReaderEventHandler>();
            }
            else
                logger.Log(LogLevel.Error, "You can not connect to a remote Andy X, configure Andy X Storage First");

            return app;
        }
    }
}
