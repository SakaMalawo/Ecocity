using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoCity.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        
        public string? Address { get; set; }
        
        public string? City { get; set; }
        
        public string? PostalCode { get; set; }
        
        public string? ProfileImageUrl { get; set; }
        
        public string? Bio { get; set; }
        
        public string? Location { get; set; }
        
        public string? ProfilePicture { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Initiative> Initiatives { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }
    }
}
