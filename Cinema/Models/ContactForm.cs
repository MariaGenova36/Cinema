using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class ContactForm
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }
    }
}