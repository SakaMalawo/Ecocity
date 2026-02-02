using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EcoCity.Models
{
    public class Vote
    {
        public int Id { get; set; }
        
        public bool IsUpvote { get; set; } // true = vote positif, false = vote négatif
        
        // Relations
        public int InitiativeId { get; set; }
        
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; }
        
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        // Empêche un utilisateur de voter plusieurs fois pour la même initiative
        [Required]
        [StringLength(450)]
        public string UserInitiativeKey { get; set; }
    }
}
