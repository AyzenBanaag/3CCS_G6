using System.ComponentModel.DataAnnotations;

namespace BarangayBayanihanOnline.Models
{
    public class ContactForm
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        public required string Message { get; set; }
    }
}
