using System.ComponentModel.DataAnnotations;

namespace MandarinBid.Models
{
    public class Bid
    {
        // первичный ключ
        public int Id { get; set; }

        // внешний ключ к мандарину (лоту)
        public int MandarinId { get; set; }

        // id пользователя (из identity)
        [Required]
        public string UserId { get; set; }

        // username пользователя (сохраняем отдельно для удобства отображения)
        public string UserName { get; set; }

        // сумма ставки
        public decimal Amount { get; set; }

        // время создания ставки (utc)
        public DateTimeOffset CreatedAt { get; set; }

        // навигационное свойство (связь many-to-one)
        public Mandarin Mandarin { get; set; }
    }
}