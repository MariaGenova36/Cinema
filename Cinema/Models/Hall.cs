using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Hall
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int SeatCount { get; set; }

        public ICollection<Projection>? Projections { get; set; }
    }
}
