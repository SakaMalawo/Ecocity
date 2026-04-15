using System.ComponentModel.DataAnnotations;

namespace EcoCity.ViewModels
{
    public class SettingsViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Nom d'utilisateur")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Prénom")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Nom")]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Téléphone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Recevoir des notifications par email")]
        public bool ReceiveEmailNotifications { get; set; } = true;

        [Display(Name = "Message de statut")]
        public string StatusMessage { get; set; } = string.Empty;
    }
}
