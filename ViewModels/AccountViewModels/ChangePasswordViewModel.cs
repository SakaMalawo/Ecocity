using System.ComponentModel.DataAnnotations;

namespace EcoCity.Models.AccountViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Le mot de passe actuel est requis")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [StringLength(100, ErrorMessage = "Le {0} doit contenir au moins {2} et au maximum {1} caractères.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [Compare("NewPassword", ErrorMessage = "Le mot de passe et la confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }
    }
}
