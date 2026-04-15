using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoCity.Data;
using EcoCity.Models;
using EcoCity.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using EcoCity.Services;

namespace EcoCity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INotificationService _notificationService;
        private readonly IAdminService _adminService;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            INotificationService notificationService,
            IAdminService adminService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _notificationService = notificationService;
            _adminService = adminService;
        }

        [Route("admin/dashboard")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new AdminDashboardViewModel
                {
                    // Statistiques générales
                    TotalUsers = await _userManager.Users.CountAsync(),
                    TotalInitiatives = await _context.Initiatives.CountAsync(),
                    TotalComments = await _context.Comments.CountAsync(),
                    TotalVotes = await _context.Votes.CountAsync(),
                    
                    // Statistiques d'approbation
                    PendingInitiatives = await _context.Initiatives.CountAsync(i => i.Status == "En attente"),
                    ApprovedInitiatives = await _context.Initiatives.CountAsync(i => i.Status == "Approuvée"),
                    RejectedInitiatives = await _context.Initiatives.CountAsync(i => i.Status == "Rejetée"),
                    
                    // Initiatives récentes en attente
                    PendingInitiativesList = await _context.Initiatives
                        .Where(i => i.Status == "En attente")
                        .Include(i => i.User)
                        .Include(i => i.Category)
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(10)
                        .Select(i => new InitiativeApprovalViewModel
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Description = i.Description,
                            Location = i.Location,
                            ImageUrl = i.ImageUrl,
                            CreatedAt = i.CreatedAt,
                            TargetDate = i.TargetDate,
                            Status = i.Status,
                            VotesCount = i.VotesCount,
                            CategoryId = i.CategoryId,
                            CategoryName = i.Category.Name,
                            UserId = i.UserId,
                            UserName = i.User.UserName,
                            UserEmail = i.User.Email,
                            CommentsCount = _context.Comments.Count(c => c.InitiativeId == i.Id)
                        })
                        .ToListAsync(),
                    
                    // Initiatives récemment approuvées/rejetées
                    RecentReviews = await _context.Initiatives
                        .Where(i => i.Status == "Approuvée" || i.Status == "Rejetée")
                        .Include(i => i.User)
                        .Include(i => i.Category)
                        .OrderByDescending(i => i.UpdatedAt ?? i.CreatedAt)
                        .Take(10)
                        .Select(i => new InitiativeApprovalViewModel
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Description = i.Description,
                            Location = i.Location,
                            ImageUrl = i.ImageUrl,
                            CreatedAt = i.CreatedAt,
                            TargetDate = i.TargetDate,
                            Status = i.Status,
                            VotesCount = i.VotesCount,
                            CategoryId = i.CategoryId,
                            CategoryName = i.Category.Name,
                            UserId = i.UserId,
                            UserName = i.User.UserName,
                            UserEmail = i.User.Email,
                            CommentsCount = _context.Comments.Count(c => c.InitiativeId == i.Id),
                            ReviewedAt = i.UpdatedAt,
                            ReviewedBy = i.ReviewedBy
                        })
                        .ToListAsync(),
                    
                    // Statistiques par catégorie
                    InitiativesByCategory = await _context.Initiatives
                        .Include(i => i.Category)
                        .GroupBy(i => i.Category.Name)
                        .ToDictionaryAsync(g => g.Key, g => g.Count()),
                    
                    // Top initiatives (uniquement approuvées)
                    TopVotedInitiatives = await _context.Initiatives
                        .Where(i => i.Status == "Approuvée")
                        .Include(i => i.User)
                        .Include(i => i.Category)
                        .OrderByDescending(i => i.VotesCount)
                        .Take(5)
                        .Select(i => new InitiativeApprovalViewModel
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Description = i.Description,
                            Location = i.Location,
                            ImageUrl = i.ImageUrl,
                            CreatedAt = i.CreatedAt,
                            Status = i.Status,
                            VotesCount = i.VotesCount,
                            CategoryId = i.CategoryId,
                            CategoryName = i.Category.Name,
                            UserId = i.UserId,
                            UserName = i.User.UserName,
                            UserEmail = i.User.Email,
                            CommentsCount = _context.Comments.Count(c => c.InitiativeId == i.Id)
                        })
                        .ToListAsync(),
                    
                    // Utilisateurs actifs
                    ActiveUsers = await GetActiveUsers()
                };

                // Calculer les statistiques mensuelles
                model.MonthlyStats = await GetMonthlyStats();
                
                // Calculer les initiatives du mois et du mois dernier
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;
                
                model.InitiativesThisMonth = await _context.Initiatives
                    .Where(i => i.CreatedAt.Month == currentMonth && i.CreatedAt.Year == currentYear)
                    .CountAsync();
                    
                model.InitiativesLastMonth = await _context.Initiatives
                    .Where(i => i.CreatedAt.Month == lastMonth && i.CreatedAt.Year == lastMonthYear)
                    .CountAsync();

                // Calculer le temps moyen de réponse
                var reviewedInitiatives = await _context.Initiatives
                    .Where(i => i.Status != "En attente" && i.UpdatedAt.HasValue)
                    .ToListAsync();
                    
                if (reviewedInitiatives.Any())
                {
                    model.AverageReviewTimeHours = reviewedInitiatives
                        .Average(i => (i.UpdatedAt.Value - i.CreatedAt).TotalHours);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // Logger l'erreur
                Console.WriteLine($"Error in Dashboard Index: {ex.Message}");
                ModelState.AddModelError("", "Une erreur est survenue lors du chargement du dashboard.");
                
                // Retourner un modèle vide pour éviter l'erreur complète
                return View(new AdminDashboardViewModel());
            }
        }

        // GET: admin/initiatives/pending
        [Route("admin/initiatives/pending")]
        public async Task<IActionResult> PendingInitiatives()
        {
            var initiatives = await _context.Initiatives
                .Where(i => i.Status == "En attente")
                .Include(i => i.User)
                .Include(i => i.Category)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InitiativeApprovalViewModel
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    Location = i.Location,
                    ImageUrl = i.ImageUrl,
                    CreatedAt = i.CreatedAt,
                    TargetDate = i.TargetDate,
                    Status = i.Status,
                    VotesCount = i.VotesCount,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category.Name,
                    UserId = i.UserId,
                    UserName = i.User.UserName,
                    UserEmail = i.User.Email,
                    CommentsCount = _context.Comments.Count(c => c.InitiativeId == i.Id)
                })
                .ToListAsync();

            return View(initiatives);
        }

        // POST: admin/initiatives/approve/5
        [HttpPost]
        [Route("admin/initiatives/approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInitiative(int id)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null)
            {
                return Json(new { success = false, message = "Initiative non trouvée." });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            
            initiative.Status = "Approuvée";
            initiative.UpdatedAt = DateTime.UtcNow;
            initiative.ReviewedBy = currentUser?.UserName;

            await _context.SaveChangesAsync();

            // Envoyer une notification à l'utilisateur
            await _notificationService.SendInitiativeApprovedNotificationAsync(
                initiative.UserId,
                initiative.Id,
                initiative.Title
            );

            // Mettre à jour les statistiques de l'admin
            var currentUserId = _userManager.GetUserId(User);
            var admin = await _adminService.GetAdminByUserIdAsync(currentUserId);
            if (admin != null)
            {
                await _adminService.UpdateAdminStatsAsync(admin.Id, "approve");
            }

            return Json(new { 
                success = true, 
                message = "Initiative approuvée avec succès.",
                newStatus = "Approuvée"
            });
        }

        // POST: admin/initiatives/reject/5
        [HttpPost]
        [Route("admin/initiatives/reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInitiative(int id, string reason)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null)
            {
                return Json(new { success = false, message = "Initiative non trouvée." });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            
            initiative.Status = "Rejetée";
            initiative.UpdatedAt = DateTime.UtcNow;
            initiative.ReviewedBy = currentUser?.UserName;
            initiative.RejectionReason = reason;

            await _context.SaveChangesAsync();

            // Envoyer une notification à l'utilisateur
            await _notificationService.SendInitiativeRejectedNotificationAsync(
                initiative.UserId,
                initiative.Id,
                initiative.Title,
                reason
            );

            // Mettre à jour les statistiques de l'admin
            var currentUserId = _userManager.GetUserId(User);
            var admin = await _adminService.GetAdminByUserIdAsync(currentUserId);
            if (admin != null)
            {
                await _adminService.UpdateAdminStatsAsync(admin.Id, "reject");
            }

            return Json(new { 
                success = true, 
                message = "Initiative rejetée avec succès.",
                newStatus = "Rejetée"
            });
        }

        // GET: admin/initiatives/details/5
        [Route("admin/initiatives/details/{id}")]
        public async Task<IActionResult> InitiativeDetails(int id)
        {
            var initiative = await _context.Initiatives
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.User)
                .Include(i => i.Votes)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
            {
                return NotFound();
            }

            var model = new InitiativeApprovalViewModel
            {
                Id = initiative.Id,
                Title = initiative.Title,
                Description = initiative.Description,
                Location = initiative.Location,
                ImageUrl = initiative.ImageUrl,
                CreatedAt = initiative.CreatedAt,
                TargetDate = initiative.TargetDate,
                Status = initiative.Status,
                VotesCount = initiative.VotesCount,
                CategoryId = initiative.CategoryId,
                CategoryName = initiative.Category.Name,
                UserId = initiative.UserId,
                UserName = initiative.User.UserName,
                UserEmail = initiative.User.Email,
                CommentsCount = initiative.Comments.Count,
                RejectionReason = initiative.RejectionReason,
                ReviewedAt = initiative.UpdatedAt,
                ReviewedBy = initiative.ReviewedBy
            };

            return View(model);
        }

        private async Task<List<UserActivityStats>> GetActiveUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            
            var userStats = new List<UserActivityStats>();
            
            foreach (var user in users)
            {
                var initiativesCount = await _context.Initiatives.CountAsync(i => i.UserId == user.Id);
                var commentsCount = await _context.Comments.CountAsync(c => c.UserId == user.Id);
                var votesCount = await _context.Votes.CountAsync(v => v.UserId == user.Id);
                
                var lastActivity = await GetLastUserActivity(user.Id);
                
                userStats.Add(new UserActivityStats
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    InitiativesCreated = initiativesCount,
                    CommentsPosted = commentsCount,
                    VotesGiven = votesCount,
                    LastActivity = lastActivity
                });
            }
            
            return userStats.OrderByDescending(u => u.ActivityScore).Take(10).ToList();
        }

        private async Task<DateTime> GetLastUserActivity(string userId)
        {
            var lastInitiative = await _context.Initiatives
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();
                
            var lastComment = await _context.Comments
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
                
            var lastVote = await _context.Votes
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
            
            var dates = new List<DateTime>();
            
            if (lastInitiative != null) dates.Add(lastInitiative.CreatedAt);
            if (lastComment != null) dates.Add(lastComment.CreatedAt);
            if (lastVote != null) dates.Add(lastVote.CreatedAt);
            
            return dates.Any() ? dates.Max() : DateTime.MinValue;
        }

        private async Task<List<MonthlyStats>> GetMonthlyStats()
        {
            var stats = new List<MonthlyStats>();
            var currentDate = DateTime.Now;
            
            for (int i = 5; i >= 0; i--)
            {
                var month = currentDate.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                var initiatives = await _context.Initiatives
                    .Where(i => i.CreatedAt >= monthStart && i.CreatedAt <= monthEnd)
                    .ToListAsync();
                    
                var approved = initiatives.Count(i => i.Status == "Approuvée");
                var rejected = initiatives.Count(i => i.Status == "Rejetée");
                var created = initiatives.Count;
                
                var newUsers = await _userManager.Users
                    .Where(u => u.CreatedAt >= monthStart && u.CreatedAt <= monthEnd)
                    .CountAsync();
                
                stats.Add(new MonthlyStats
                {
                    Month = month.ToString("MMM yyyy"),
                    InitiativesCreated = created,
                    InitiativesApproved = approved,
                    InitiativesRejected = rejected,
                    NewUsers = newUsers
                });
            }
            
            return stats;
        }
    }
}
