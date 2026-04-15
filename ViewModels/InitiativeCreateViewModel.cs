using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EcoCity.ViewModels
{
    public class InitiativeCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le titre est requis")]
        [StringLength(100, ErrorMessage = "Le titre ne peut pas dépasser 100 caractères")]
        public string Title { get; set; }

        [Required(ErrorMessage = "La description est requise")]
        public string Description { get; set; }

        [Required(ErrorMessage = "L'emplacement est requis")]
        public string Location { get; set; }

        [Display(Name = "Date cible")]
        [DataType(DataType.Date)]
        public System.DateTime? TargetDate { get; set; }

        public string Status { get; set; } = "Actif";

        [Required(ErrorMessage = "Les objectifs sont requis")]
        public string Goals { get; set; }

        [Required(ErrorMessage = "Les ressources requises sont obligatoires")]
        public string RequiredResources { get; set; }

        [Display(Name = "Budget (¤)")]
        [Range(0, double.MaxValue, ErrorMessage = "Le budget doit être positif")]
        public decimal? Budget { get; set; }

        [Required(ErrorMessage = "La durée est requise")]
        public string Duration { get; set; }

        [Required(ErrorMessage = "Les compétences requises sont obligatoires")]
        public string RequiredSkills { get; set; }

        [Display(Name = "Image")]
        public IFormFile ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Required(ErrorMessage = "La catégorie est requise")]
        public int CategoryId { get; set; }
    }
}
