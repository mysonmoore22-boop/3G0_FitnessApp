using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
