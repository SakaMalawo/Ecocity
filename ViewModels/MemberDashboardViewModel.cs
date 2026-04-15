using EcoCity.Models;

namespace EcoCity.ViewModels
{
    public class MemberDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public IEnumerable<Initiative> RecentInitiatives { get; set; } = new List<Initiative>();
        public IEnumerable<Initiative> SupportedInitiatives { get; set; } = new List<Initiative>();
        public int TotalInitiatives { get; set; }
        public int TotalVotes { get; set; }
        public int PendingInitiatives { get; set; }
        public int ApprovedInitiatives { get; set; }
        public int RejectedInitiatives { get; set; }
        public int UnreadNotifications { get; set; }
        public IEnumerable<Notification> RecentNotifications { get; set; } = new List<Notification>();
        
        // Propriétés manquantes
        public int TotalSupported { get; set; }
        public int TotalComments { get; set; }
        public int UnreadMessages { get; set; }
        public int SupportersCount { get; set; }
    }
}
