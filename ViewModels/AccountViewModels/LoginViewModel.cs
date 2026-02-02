using System.ComponentModel.DataAnnotations;

namespace EcoCity.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Veuillez entrer une adresse email valide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Se souvenir de moi ?")]
        public bool RememberMe { get; set; }
    }
}
