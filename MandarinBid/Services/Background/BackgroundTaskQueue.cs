using System.Threading.Channels;

namespace MandarinBid.Services.Background
{
    // интерфейс очереди фоновых задач (producer-consumer паттерн)
    public interface IBackgroundTaskQueue
    {
        // добавление задачи в очередь
        void Queue(Func<CancellationToken, Task> workItem);

        // извлечение задачи из очереди (ожидание, если очередь пуста)
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken token);
    }

    // реализация очереди на основе Channel (thread-safe и высокопроизводительно)
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        // неблокирующая очередь без ограничений по размеру
        private readonly Channel<Func<CancellationToken, Task>> _queue =
            Channel.CreateUnbounded<Func<CancellationToken, Task>>();

        /// <summary>
        /// добавляет задачу в очередь фоновой обработки
        /// </summary>
        /// <param name="workItem">асинхронная задача</param>
        public void Queue(Func<CancellationToken, Task> workItem)
        {
            // trywrite — не блокирует поток (важно для производительности)
            _queue.Writer.TryWrite(workItem);
        }

        /// <summary>
        /// извлекает задачу из очереди
        /// </summary>
        /// <param name="token">токен отмены</param>
        /// <returns>задача для выполнения</returns>
        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken token)
        {
            return await _queue.Reader.ReadAsync(token);
        }
    }
}