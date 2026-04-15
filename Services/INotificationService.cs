using EcoCity.Models;

namespace EcoCity.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string type, string relatedEntityType = "", int? relatedEntityId = null, string actionUrl = "");
        Task SendInitiativeSubmittedNotificationAsync(string userId, int initiativeId, string initiativeTitle);
        Task SendInitiativeApprovedNotificationAsync(string userId, int initiativeId, string initiativeTitle);
        Task SendInitiativeRejectedNotificationAsync(string userId, int initiativeId, string initiativeTitle, string rejectionReason);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task MarkNotificationAsReadAsync(int notificationId, string userId);
        Task MarkAllNotificationsAsReadAsync(string userId);
        Task<int> GetUnreadNotificationsCountAsync(string userId);
        Task DeleteNotificationAsync(int notificationId, string userId);
    }
}
