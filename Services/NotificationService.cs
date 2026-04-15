using EcoCity.Models;
using EcoCity.Data;
using Microsoft.EntityFrameworkCore;

namespace EcoCity.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, string type, string relatedEntityType = "", int? relatedEntityId = null, string actionUrl = "")
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                ActionUrl = actionUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task SendInitiativeSubmittedNotificationAsync(string userId, int initiativeId, string initiativeTitle)
        {
            await SendNotificationAsync(
                userId,
                "Initiative soumise pour approbation",
                $"Votre initiative '{initiativeTitle}' a été soumise et est en attente d'approbation par notre équipe de modération.",
                "Info",
                "Initiative",
                initiativeId,
                $"/initiative/details/{initiativeId}"
            );
        }

        public async Task SendInitiativeApprovedNotificationAsync(string userId, int initiativeId, string initiativeTitle)
        {
            await SendNotificationAsync(
                userId,
                "Initiative approuvée !",
                $"Félicitations ! Votre initiative '{initiativeTitle}' a été approuvée et est maintenant visible par tous les utilisateurs.",
                "Success",
                "Initiative",
                initiativeId,
                $"/initiative/details/{initiativeId}"
            );
        }

        public async Task SendInitiativeRejectedNotificationAsync(string userId, int initiativeId, string initiativeTitle, string rejectionReason)
        {
            await SendNotificationAsync(
                userId,
                "Initiative rejetée",
                $"Votre initiative '{initiativeTitle}' n'a pas pu être approuvée. Raison : {rejectionReason}",
                "Warning",
                "Initiative",
                initiativeId,
                $"/initiative/details/{initiativeId}"
            );
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkNotificationAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadNotificationsCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}
