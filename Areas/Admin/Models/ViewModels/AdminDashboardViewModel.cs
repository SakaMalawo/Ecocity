using EcoCity.Models;

namespace EcoCity.Areas.Admin.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalInitiatives { get; set; }
        public int PendingInitiatives { get; set; }
        public int ApprovedInitiatives { get; set; }
        public int RejectedInitiatives { get; set; }
        public int TotalVotes { get; set; }
        public List<UserActivityStats> ActiveUsers { get; set; } = new List<UserActivityStats>();
        public int NewUsersThisMonth { get; set; }
        public int NewInitiativesThisMonth { get; set; }
        public IEnumerable<InitiativeApprovalViewModel> RecentInitiatives { get; set; } = new List<InitiativeApprovalViewModel>();
        public IEnumerable<InitiativeApprovalViewModel> PendingInitiativesList { get; set; } = new List<InitiativeApprovalViewModel>();
        public IEnumerable<UserActivityStats> UserActivityStats { get; set; } = new List<UserActivityStats>();
        public IEnumerable<MonthlyStats> MonthlyStats { get; set; } = new List<MonthlyStats>();
        public IEnumerable<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
        
        // Propriétés manquantes
        public int TotalComments { get; set; }
        public IEnumerable<InitiativeApprovalViewModel> RecentReviews { get; set; } = new List<InitiativeApprovalViewModel>();
        public Dictionary<string, int> InitiativesByCategory { get; set; } = new Dictionary<string, int>();
        public IEnumerable<InitiativeApprovalViewModel> TopVotedInitiatives { get; set; } = new List<InitiativeApprovalViewModel>();
        public int InitiativesThisMonth { get; set; }
        public int InitiativesLastMonth { get; set; }
        public double AverageReviewTimeHours { get; set; }
        public int ApprovalRate { get; set; }
        public double MonthlyGrowthRate { get; set; }
    }

    public class UserActivityStats
    {
        public string UserName { get; set; } = string.Empty;
        public int InitiativesCount { get; set; }
        public int VotesCount { get; set; }
        public DateTime LastActivity { get; set; }
        
        // Propriétés manquantes
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int InitiativesCreated { get; set; }
        public int CommentsPosted { get; set; }
        public int VotesGiven { get; set; }
        public int ActivityScore { get; set; }
    }

    public class MonthlyStats
    {
        public string Month { get; set; } = string.Empty;
        public int InitiativesCount { get; set; }
        public int UsersCount { get; set; }
        public int VotesCount { get; set; }
        
        // Propriétés manquantes
        public int InitiativesCreated { get; set; }
        public int InitiativesApproved { get; set; }
        public int InitiativesRejected { get; set; }
        public int NewUsers { get; set; }
    }
}
