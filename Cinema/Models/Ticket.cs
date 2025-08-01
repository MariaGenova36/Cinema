using System;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public int ProjectionId { get; set; }
        public Projection Projection { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        public DateTime PurchaseTime { get; set; } = DateTime.UtcNow;
    }
}
