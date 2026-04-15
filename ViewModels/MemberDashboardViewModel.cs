using EcoCity.Models;

namespace EcoCity.ViewModels
{
    public class MemberDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        
        public IEnumerable<Initiative> RecentInitiatives { get; set; } = new List<Initiative>();
        public IEnumerable<Comment> RecentComments { get; set; } = new List<Comment>();
        public IEnumerable<Initiative> SupportedInitiatives { get; set; } = new List<Initiative>();
        
        public int TotalInitiatives { get; set; }
        public int PublishedInitiatives { get; set; }
        public int PendingInitiatives { get; set; }
        public int ApprovedInitiatives { get; set; }
        public int RejectedInitiatives { get; set; }
        public int TotalVotes { get; set; }
        public int TotalComments { get; set; }
        public int UnreadNotifications { get; set; }
        public int UnreadMessages { get; set; }
        public int TotalSupported { get; set; }
        public int SupportersCount { get; set; }
        
        public IEnumerable<Notification> RecentNotifications { get; set; } = new List<Notification>();
    }
}
