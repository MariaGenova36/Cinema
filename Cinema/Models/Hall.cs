using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Hall
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(1, 50)]
        public int Rows{ get; set; }    // Брой редове

        [Range(1, 50)]
        public int Columns { get; set; } // Брой колони (места на ред)

        public ICollection<Projection>? Projections { get; set; }
    }
}
