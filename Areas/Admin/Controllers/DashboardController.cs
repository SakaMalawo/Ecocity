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
using EcoCity.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcoCity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Route("admin")]
        [Route("admin/dashboard")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new DashboardViewModel
                {
                    TotalUsers = await _userManager.Users.CountAsync(),
                    TotalInitiatives = await _context.Initiatives.CountAsync(),
                    TotalCategories = await _context.Categories.CountAsync(),
                    TotalComments = await _context.Comments.CountAsync(),
                    RecentUsers = await _userManager.Users
                        .OrderByDescending(u => u.CreatedAt)
                        .Take(5)
                        .ToListAsync(),
                    RecentInitiatives = await _context.Initiatives
                        .Include(i => i.User)
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(5)
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log l'erreur
                _context.Logs.Add(new Log
                {
                    Message = $"Erreur lors du chargement du tableau de bord: {ex.Message}",
                    Level = "Error",
                    Exception = ex.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du tableau de bord.";
                return View(new DashboardViewModel());
            }
        }

        [HttpGet("admin/statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var today = DateTime.Today;
                var startDate = today.AddDays(-30);

                var userRegistrations = await _userManager.Users
                    .Where(u => u.CreatedAt >= startDate)
                    .GroupBy(u => u.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var initiativeCreations = await _context.Initiatives
                    .Where(i => i.CreatedAt >= startDate)
                    .GroupBy(i => i.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                return Json(new
                {
                    Success = true,
                    UserRegistrations = userRegistrations,
                    InitiativeCreations = initiativeCreations
                });
            }
            catch (Exception ex)
            {
                // Log l'erreur
                _context.Logs.Add(new Log
                {
                    Message = $"Erreur lors de la récupération des statistiques: {ex.Message}",
                    Level = "Error",
                    Exception = ex.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Json(new { Success = false, Message = "Une erreur est survenue lors de la récupération des statistiques." });
            }
        }

        [HttpGet("admin/users")]
        public async Task<IActionResult> Users(int? page, string search = null, string role = null, string sortOrder = "newest")
        {
            const int pageSize = 10;
            var pageNumber = page ?? 1;

            var query = _userManager.Users.AsQueryable();

            // Filtrage
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => 
                    u.FirstName.Contains(search) || 
                    u.LastName.Contains(search) || 
                    u.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIds = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIds.Contains(u.Id));
            }

            // Tri
            query = sortOrder switch
            {
                "name_asc" => query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName),
                "name_desc" => query.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName),
                "email" => query.OrderBy(u => u.Email),
                "newest" => query.OrderByDescending(u => u.CreatedAt),
                "oldest" => query.OrderBy(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            var users = await PaginatedList<ApplicationUser>.CreateAsync(query, pageNumber, pageSize);

            var model = new UserListViewModel
            {
                Users = users,
                SearchTerm = search,
                SelectedRole = role,
                SortOrder = sortOrder,
                Roles = _roleManager.Roles.Select(r => r.Name ?? string.Empty).ToList()
            };

            return View(model);
        }

        [HttpPost("admin/users/{id}/toggle-status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Le statut de l'utilisateur a été mis à jour avec succès.";
            }
            else
            {
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la mise à jour du statut de l'utilisateur.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost("admin/users/{id}/roles")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(string id, [FromForm] List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (result.Succeeded && roles != null)
            {
                result = await _userManager.AddToRolesAsync(user, roles);
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Les rôles de l'utilisateur ont été mis à jour avec succès.";
            }
            else
            {
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la mise à jour des rôles de l'utilisateur.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}
