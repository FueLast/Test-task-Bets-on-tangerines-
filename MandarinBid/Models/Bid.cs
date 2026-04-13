using System.ComponentModel.DataAnnotations;

namespace MandarinBid.Models
{
    public class Bid
    {
        public int Id { get; set; }

        public int MandarinId { get; set; } // foreign key

        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }

        public decimal Amount { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        // навигация
        public Mandarin Mandarin { get; set; }
    }
}