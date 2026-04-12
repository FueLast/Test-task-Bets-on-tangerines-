using System.ComponentModel.DataAnnotations;

namespace MandarinBid.Models
{
    public class Mandarin
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal CurrentPrice { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ExpirationDate { get; set; } // дата окончания аукциона 

        public string? ImageUrl { get; set; }

        // для конкуренции (важно для аукциона)
        [Timestamp]
        public byte[] RowVersion { get; set; }

        // навигация к ставкам (связь 1 ко многим)
        public List<Bid> Bids { get; set; } = new();

        public bool IsProcessed { get; set; }
    }
}