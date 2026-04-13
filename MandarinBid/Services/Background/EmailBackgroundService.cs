namespace MandarinBid.Services.Background
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly ILogger<EmailBackgroundService> _logger; 

        public EmailBackgroundService(
            IBackgroundTaskQueue queue,
            ILogger<EmailBackgroundService> logger) 
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        { 
            _logger.LogInformation("Email Background Service started at {Time}", DateTimeOffset.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            { 
                var workItem = await _queue.DequeueAsync(stoppingToken);

                try
                { 
                    _logger.LogInformation("Executing email background task");

                    await workItem(stoppingToken);

                    _logger.LogInformation("Email background task completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing email background task");
                }
            }

            _logger.LogInformation("Email Background Service is stopping.");
        }
    }
}
