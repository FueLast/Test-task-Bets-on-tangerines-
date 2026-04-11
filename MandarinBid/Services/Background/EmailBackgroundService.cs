

namespace MandarinBid.Services.Background
{    public class EmailBackgroundService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;

        public EmailBackgroundService(IBackgroundTaskQueue queue)
        {
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);
                await workItem(stoppingToken);
            }
        }
    }
}
