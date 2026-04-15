using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EcoCity.Models
{
    public class Initiative
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public string Location { get; set; }
        
        public string ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? TargetDate { get; set; }
        
        public string Status { get; set; } = "En attente"; // En attente, Approuvée, En cours, Terminée, Rejetée
        
        public int VotesCount { get; set; } = 0;
        
        // Champs pour le suivi d'approbation
        public DateTime? UpdatedAt { get; set; }
        
        public string? ReviewedBy { get; set; }
        
        [StringLength(500)]
        public string? RejectionReason { get; set; }
        
        public string Goals { get; set; }
        
        public string RequiredResources { get; set; }
        
        public decimal? Budget { get; set; }
        
        public string Duration { get; set; }
        
        public string RequiredSkills { get; set; }
        
        // Relations
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        
        // Navigation properties
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }
    }
}
