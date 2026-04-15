using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoCity.Data;
using EcoCity.Models;
using EcoCity.Models.AccountViewModels;
using EcoCity.ViewModels;
using System.Threading.Tasks;

namespace EcoCity.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Member/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);

            if (userId == null || user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new MemberDashboardViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfilePicture ?? "/images/default-avatar.svg",
                
                // Statistiques de l'utilisateur
                TotalInitiatives = await _context.Initiatives.CountAsync(i => i.UserId == userId),
                PublishedInitiatives = await _context.Initiatives.CountAsync(i => i.UserId == userId && i.Status == "Approuvée"),
                PendingInitiatives = await _context.Initiatives.CountAsync(i => i.UserId == userId && i.Status == "En attente"),
                TotalComments = await _context.Comments.CountAsync(c => c.UserId == userId),
                TotalVotes = await _context.Votes.CountAsync(v => v.UserId == userId),
                
                // Initiatives récentes
                RecentInitiatives = await _context.Initiatives
                    .Where(i => i.UserId == userId)
                    .Include(i => i.Category)
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                    
                // Commentaires récents
                RecentComments = await _context.Comments
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Initiative)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        // GET: /Member/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // POST: /Member/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Mettre à jour les informations de l'utilisateur
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.PostalCode = model.PostalCode;
            user.Bio = model.Bio;

            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil mis à jour avec succès.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        // GET: /Member/MyInitiatives
        public async Task<IActionResult> MyInitiatives()
        {
            var userId = _userManager.GetUserId(User);
            
            var initiatives = await _context.Initiatives
                .Where(i => i.UserId == userId)
                .Include(i => i.Category)
                .Include(i => i.User)
                .Include(i => i.Comments)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(initiatives);
        }

        // GET: /Member/Settings
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new SettingsViewModel
            {
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? "",
                ReceiveEmailNotifications = true,
                StatusMessage = ""
            };

            return View(model);
        }

        // POST: /Member/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Mettre àjour les paramètres
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                model.StatusMessage = "Paramètres mis à jour avec succès.";
                return RedirectToAction(nameof(Settings));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Member/SupportedInitiatives
        public async Task<IActionResult> SupportedInitiatives()
        {
            var userId = _userManager.GetUserId(User);
            
            var votedInitiativeIds = await _context.Votes
                .Where(v => v.UserId == userId)
                .Select(v => v.InitiativeId)
                .ToListAsync();

            var initiatives = await _context.Initiatives
                .Where(i => votedInitiativeIds.Contains(i.Id))
                .Include(i => i.Category)
                .Include(i => i.User)
                .Include(i => i.Comments)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(initiatives);
        }

        // GET: /Member/Messages
        public async Task<IActionResult> Messages()
        {
            var userId = _userManager.GetUserId(User);
            
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();

            var messages = notifications.Select(n => new MessageViewModel
            {
                Title = n.Title,
                Content = n.Message,
                SenderName = "EcoCity",
                SenderAvatar = "/images/default-avatar.svg",
                Subject = n.Title,
                SentAt = n.CreatedAt,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                MessageType = n.Type ?? "Info"
            }).ToList();

            return View(messages);
        }

        // GET: /Member/Notifications
        public async Task<IActionResult> Notifications()
        {
            var userId = _userManager.GetUserId(User);
            
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return View(notifications);
        }

        // GET: /Member/Statistics
        public async Task<IActionResult> Statistics()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);

            if (userId == null || user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new MemberDashboardViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfilePicture ?? "/images/default-avatar.svg",
                TotalInitiatives = await _context.Initiatives.CountAsync(i => i.UserId == userId),
                PublishedInitiatives = await _context.Initiatives.CountAsync(i => i.UserId == userId && i.Status == "Approuvée"),
                PendingInitiatives = await _context.Initiatives.CountAsync(i => i.UserId == userId && i.Status == "En attente"),
                TotalComments = await _context.Comments.CountAsync(c => c.UserId == userId),
                TotalVotes = await _context.Votes.CountAsync(v => v.UserId == userId)
            };

            return View(model);
        }

        // POST: /Member/MarkNotificationAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead([FromBody] MarkNotificationRequest request)
        {
            var notification = await _context.Notifications.FindAsync(request.NotificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }

    public class MarkNotificationRequest
    {
        public int NotificationId { get; set; }
    }
}
