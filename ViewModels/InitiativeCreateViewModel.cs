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
        [StringLength(5000, ErrorMessage = "La description ne peut pas dépasser 5000 caractères")]
        public string Description { get; set; }

        [StringLength(200, ErrorMessage = "L'emplacement ne peut pas dépasser 200 caractères")]
        public string Location { get; set; }

        [Display(Name = "Date cible")]
        [DataType(DataType.Date)]
        public System.DateTime? TargetDate { get; set; }

        [Display(Name = "Statut")]
        public string Status { get; set; }

        [StringLength(1000, ErrorMessage = "Les objectifs ne peuvent pas dépasser 1000 caractères")]
        public string Goals { get; set; }

        [StringLength(1000, ErrorMessage = "Les ressources requises ne peuvent pas dépasser 1000 caractères")]
        public string RequiredResources { get; set; }

        [Display(Name = "Budget (€)")]
        [Range(0, double.MaxValue, ErrorMessage = "Le budget doit être positif")]
        public decimal? Budget { get; set; }

        [StringLength(100, ErrorMessage = "La durée ne peut pas dépasser 100 caractères")]
        public string Duration { get; set; }

        [StringLength(500, ErrorMessage = "Les compétences requises ne peuvent pas dépasser 500 caractères")]
        public string RequiredSkills { get; set; }

        [Display(Name = "Image")]
        public IFormFile ImageFile { get; set; }

        public string ImageUrl { get; set; }

        public string ExistingImageUrl { get; set; }

        public int CategoryId { get; set; }
    }
}
