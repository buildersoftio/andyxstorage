using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Microsoft.Extensions.Logging;
using EFCore.BulkExtensions;
using System.Linq;
using System.Timers;

namespace Buildersoft.Andy.X.Storage.IO.Background.Services
{
    public class ConsumerArchiveBackgroundService
    {
        private readonly ILogger<ConsumerIOService> _logger;
        private readonly string _tenant;
        private readonly string _product;
        private readonly string _component;
        private readonly string _topic;
        private readonly string _consumer;
        private readonly PartitionConfiguration _partitionConfiguration;
        private readonly ConsumerPointerContext _consumerPointerContext;

        private Timer backgroundTaskTimer;

        public ConsumerArchiveBackgroundService(ILogger<ConsumerIOService> logger,
            string tenant,
            string product,
            string component,
            string topic,
            string consumer,
            PartitionConfiguration partitionConfiguration,
            ConsumerPointerContext consumerPointerContext)
        {
            _logger = logger;
            _tenant = tenant;
            _product = product;
            _component = component;
            _topic = topic;
            _consumer = consumer;

            _partitionConfiguration = partitionConfiguration;
            _consumerPointerContext = consumerPointerContext;

            InitializeBackgroundTask();
        }

        private void InitializeBackgroundTask()
        {
            backgroundTaskTimer = new Timer();
            backgroundTaskTimer.Interval = _partitionConfiguration.PointerAcknowledgedMessageArchivationInterval;
            backgroundTaskTimer.Elapsed += BackgroundTaskTimer_Elapsed;
            backgroundTaskTimer.AutoReset = true;

            _logger.LogInformation($"Consumer pointer archivation service for '{_tenant}/{_product}/{_component}/{_topic}/{_consumer}' is initialized");
        }

        public void StopService()
        {
            if (backgroundTaskTimer != null)
                backgroundTaskTimer.Stop();
        }

        public void StartService()
        {
            if (backgroundTaskTimer != null)
                backgroundTaskTimer.Start();
        }

        private void BackgroundTaskTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogInformation($"Consumer pointer archivation service for '{_tenant}/{_product}/{_component}/{_topic}/{_consumer}' is triggered");

            StopService();

            var ackedPointerMessages = _consumerPointerContext.ConsumerMessages.Where(x => x.IsAcknowledged == true).OrderBy(x => x.SentDate).Take(_partitionConfiguration.SizeInMemory);
            _consumerPointerContext.BulkDelete(ackedPointerMessages.ToList());

            StartService();
        }
    }
}
