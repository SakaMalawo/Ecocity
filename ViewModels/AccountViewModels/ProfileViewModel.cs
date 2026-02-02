using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EcoCity.Models.AccountViewModels
{
    public class ProfileViewModel
    {
        [Display(Name = "Nom d'utilisateur")]
        public string Username { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Veuillez entrer une adresse email valide")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        [Display(Name = "Prénom")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [Display(Name = "Nom")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Veuillez entrer un numéro de téléphone valide")]
        [Display(Name = "Téléphone")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Adresse")]
        [StringLength(200, ErrorMessage = "L'adresse ne peut pas dépasser 200 caractères")]
        public string Address { get; set; }

        [Display(Name = "Ville")]
        [StringLength(100, ErrorMessage = "Le nom de la ville ne peut pas dépasser 100 caractères")]
        public string City { get; set; }

        [Display(Name = "Code postal")]
        [StringLength(20, ErrorMessage = "Le code postal ne peut pas dépasser 20 caractères")]
        public string PostalCode { get; set; }

        [Display(Name = "Photo de profil")]
        public IFormFile ProfileImage { get; set; }

        [Display(Name = "Photo de profil actuelle")]
        public string ProfileImageUrl { get; set; }

        public bool IsEmailConfirmed { get; set; }
        public string StatusMessage { get; set; }
        public ChangePasswordViewModel ChangePasswordModel { get; set; } = new ChangePasswordViewModel();
    }
}
