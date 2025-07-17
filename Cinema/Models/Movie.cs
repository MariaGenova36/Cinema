using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int GenreId { get; set; }
        public Genre? Genre { get; set; }
        [DataType(DataType.Time)]
        public TimeSpan Duration { get; set; }
        public DateTime ReleaseDate { get; set; }
        [NotMapped]
        public IFormFile PosterFile { get; set; }
        public string? PosterUrl { get; set; }
    }
}
