namespace EcoCity.ViewModels
{
    public class MessageViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string MessageType { get; set; } = string.Empty; // Info, Warning, Success, Error
        
        // Propriétés manquantes
        public string SenderAvatar { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }
}
