using Microsoft.Extensions.Logging;

namespace Storage.Core.Service.System
{
    public class SystemService
    {
        private readonly ILogger<SystemService> _logger;

        public SystemService(ILogger<SystemService> logger)
        {
            _logger = logger;

            InitializeServices();
        }

        private void InitializeServices()
        {
            _logger.LogInformation("Buildersoft");
            _logger.LogInformation("Welcome to Andy X DataStorage");


            _logger.LogInformation("andyx-storage|connecting to andy x node");
            

            _logger.LogInformation("andyx-storage|starting services");
            _logger.LogInformation("andyx-storage|starting agents");
        }
    }
}
