using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoCity.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        public string Level { get; set; } // Information, Warning, Error, etc.
        
        public string Exception { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string Source { get; set; } // Controller/Action where the log was created
        
        public string UserId { get; set; } // Optional: ID of the user who triggered the action
        
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }
}
