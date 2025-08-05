using System;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Projection
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }

        [Required]
        public int HallId { get; set; }
        public Hall? Hall { get; set; }

        [Required]
        public DateTime ProjectionTime { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal TicketPrice { get; set; }
    }
}
