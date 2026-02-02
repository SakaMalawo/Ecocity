using System.ComponentModel.DataAnnotations;

namespace EcoCity.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le prénom est requis")]
        [Display(Name = "Prénom")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [Display(Name = "Nom")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        [Display(Name = "Nom d'utilisateur")]
        [StringLength(50, ErrorMessage = "Le nom d'utilisateur ne peut pas dépasser 50 caractères")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Veuillez entrer une adresse email valide")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [StringLength(100, ErrorMessage = "Le {0} doit contenir au moins {2} et au maximum {1} caractères.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Le mot de passe et la confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Adresse")]
        [StringLength(200, ErrorMessage = "L'adresse ne peut pas dépasser 200 caractères")]
        public string? Address { get; set; }

        [Display(Name = "Ville")]
        [StringLength(100, ErrorMessage = "Le nom de la ville ne peut pas dépasser 100 caractères")]
        public string? City { get; set; }

        [Display(Name = "Code postal")]
        [StringLength(20, ErrorMessage = "Le code postal ne peut pas dépasser 20 caractères")]
        public string? PostalCode { get; set; }

        [Display(Name = "Bio")]
        [StringLength(500, ErrorMessage = "La bio ne peut pas dépasser 500 caractères")]
        public string? Bio { get; set; }

        [Display(Name = "Localisation")]
        [StringLength(100, ErrorMessage = "La localisation ne peut pas dépasser 100 caractères")]
        public string? Location { get; set; }

        [Display(Name = "J'accepte les conditions d'utilisation")]
        [Required(ErrorMessage = "Vous devez accepter les conditions d'utilisation")]
        public bool AcceptTerms { get; set; }

        [Display(Name = "Je souhaite recevoir la newsletter")]
        public bool SubscribeNewsletter { get; set; }
    }
}
