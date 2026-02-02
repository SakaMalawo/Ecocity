using System.ComponentModel.DataAnnotations;

namespace EcoCity.Models.AccountViewModels
{
    public class LoginWith2faViewModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "Le {0} doit comporter {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Code d'authentification")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "Se souvenir de cet ordinateur")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }
    }
}
