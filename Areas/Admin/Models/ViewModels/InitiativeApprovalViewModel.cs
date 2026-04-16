using EcoCity.Models;

namespace EcoCity.Areas.Admin.Models.ViewModels
{
    public class InitiativeApprovalViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int VotesCount { get; set; }
        public int CommentsCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Goals { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime? ReviewedAt { get; set; }
        public string ReviewedBy { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public bool IsPending { get; set; }
    }
}
