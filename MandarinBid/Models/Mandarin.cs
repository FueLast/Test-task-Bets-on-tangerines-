using System.ComponentModel.DataAnnotations;

namespace MandarinBid.Models
{
    public class Mandarin
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal CurrentPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string? ImageUrl { get; set; }

        // для оптимистичной конкуренции (очень важно для аукциона)
        [Timestamp]
        public byte[] RowVersion { get; set; }

        // навигация к ставкам
        public List<Bid> Bids { get; set; } = new();
    }
}