using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        //Свързване с потребителя
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [Range(1, 50)]
        public int SeatRow { get; set; }

        [Required]
        [Range(1, 50)]
        public int SeatColumn { get; set; }


        public string TicketType { get; set; }
        public decimal Price { get; set; }

        public bool IsPaid { get; set; } = false;
        public bool IsUsed { get; set; } = false;
    }
}
