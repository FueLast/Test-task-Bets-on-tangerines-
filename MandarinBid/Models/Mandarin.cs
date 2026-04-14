using System.ComponentModel.DataAnnotations;

namespace MandarinBid.Models
{
    public class Mandarin
    {
        // первичный ключ
        public int Id { get; set; }

        // название лота
        [Required]
        public string Name { get; set; }

        // текущая цена (последняя ставка)
        public decimal CurrentPrice { get; set; }

        // дата создания лота
        public DateTimeOffset CreatedAt { get; set; }

        // дата окончания аукциона
        public DateTimeOffset ExpirationDate { get; set; }

        // опциональное изображение
        public string? ImageUrl { get; set; }

        // optimistic concurrency token (защита от гонок при ставках)
        [Timestamp]
        public byte[] RowVersion { get; set; }

        // коллекция ставок (1 ко многим)
        public List<Bid> Bids { get; set; } = new();

        // флаг обработки (чтобы cleanup не обрабатывал повторно)
        public bool IsProcessed { get; set; }
    }
}