using System.ComponentModel.DataAnnotations;

namespace EcoCity.Areas.Admin.Models.ViewModels
{
    public class AdminSettingsViewModel
    {
        [Required(ErrorMessage = "Le titre du site est requis")]
        [StringLength(100, ErrorMessage = "Le titre ne peut pas dépasser 100 caractères")]
        [Display(Name = "Titre du site")]
        public string SiteTitle { get; set; }

        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        [Display(Name = "Description du site")]
        public string SiteDescription { get; set; }

        [Required(ErrorMessage = "Le nombre d'éléments par page est requis")]
        [Range(5, 100, ErrorMessage = "Le nombre d'éléments par page doit être compris entre 5 et 100")]
        [Display(Name = "Nombre d'éléments par page")]
        public int ItemsPerPage { get; set; }

        [Display(Name = "Autoriser les nouvelles inscriptions")]
        public bool AllowRegistrations { get; set; }

        [Display(Name = "Exiger une confirmation d'email")]
        public bool RequireEmailConfirmation { get; set; }

        [Display(Name = "Activer la modération des commentaires")]
        public bool EnableCommentModeration { get; set; }

        [Display(Name = "Activer les notifications par email")]
        public bool EnableEmailNotifications { get; set; }

        [EmailAddress(ErrorMessage = "Veuillez entrer une adresse email valide")]
        [Display(Name = "Email de contact")]
        public string ContactEmail { get; set; }
    }
}
