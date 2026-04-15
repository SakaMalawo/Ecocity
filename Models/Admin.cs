using System;

namespace EcoCity.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Moderator
        public string Department { get; set; } = string.Empty;
        public string Permissions { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int InitiativesApproved { get; set; } = 0;
        public int InitiativesRejected { get; set; } = 0;
        public int ActionsCount { get; set; } = 0;
        public DateTime? LastActionAt { get; set; }
        
        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
    }
}
