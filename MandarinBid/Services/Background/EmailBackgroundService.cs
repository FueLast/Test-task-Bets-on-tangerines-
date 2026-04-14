namespace MandarinBid.Services.Background
{
    // background service для обработки email-задач вне основного потока запроса
    public class EmailBackgroundService : BackgroundService
    {
        // очередь задач (email отправка)
        private readonly IBackgroundTaskQueue _queue;

        // логгер для мониторинга работы сервиса
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(
            IBackgroundTaskQueue queue,
            ILogger<EmailBackgroundService> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        // основной цикл фонового сервиса
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // лог старта сервиса
            _logger.LogInformation("Email Background Service started at {Time}", DateTimeOffset.UtcNow);

            // бесконечный цикл до остановки приложения
            while (!stoppingToken.IsCancellationRequested)
            {
                // ждём следующую задачу из очереди
                var workItem = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    _logger.LogInformation("Executing email background task");

                    // выполняем задачу (например отправку email)
                    await workItem(stoppingToken);

                    _logger.LogInformation("Email background task completed successfully");
                }
                catch (Exception ex)
                {
                    // важно: ошибка не должна убить сервис
                    _logger.LogError(ex, "Error occurred executing email background task");
                }
            }

            // лог остановки сервиса
            _logger.LogInformation("Email Background Service is stopping.");
        }
    }
}