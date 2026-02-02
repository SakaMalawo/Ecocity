using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EcoCity.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Relations
        public int InitiativeId { get; set; }
        
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; }
        
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
