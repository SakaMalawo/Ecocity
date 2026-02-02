using System.ComponentModel.DataAnnotations;

namespace EcoCity.Models.AccountViewModels
{
    public class LoginWithRecoveryCodeViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Code de récupération")]
        public string RecoveryCode { get; set; }
    }
}
