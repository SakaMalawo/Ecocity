using System.ComponentModel.DataAnnotations;

namespace EcoCity.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Veuillez entrer une adresse email valide")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Le sujet est requis")]
        [StringLength(200, ErrorMessage = "Le sujet ne peut pas dépasser 200 caractères")]
        public string Subject { get; set; }
        
        [Required(ErrorMessage = "Le message est requis")]
        [StringLength(5000, ErrorMessage = "Le message ne peut pas dépasser 5000 caractères")]
        public string Message { get; set; }
    }
}
