using System;

namespace EcoCity.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Info, Success, Warning, Error
        public string RelatedEntityType { get; set; } = string.Empty; // Initiative, User, etc.
        public int? RelatedEntityId { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        
        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
    }
}
