using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoCity.Data;
using EcoCity.Models;
using EcoCity.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using EcoCity.Services;

namespace EcoCity.Controllers
{
    [Authorize]
    public class InitiativeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly INotificationService _notificationService;

        public InitiativeController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _notificationService = notificationService;
        }

        // GET: Initiative
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, int? categoryId, string status, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentSort"] = sortOrder;

            var initiatives = _context.Initiatives
                .Include(i => i.User)
                .Include(i => i.Category)
                .Where(i => i.Status == "Approuvée")
                .AsQueryable();

            // Filtrage
            if (!string.IsNullOrEmpty(searchString))
            {
                initiatives = initiatives.Where(i => 
                    i.Title.Contains(searchString) || 
                    i.Description.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                initiatives = initiatives.Where(i => i.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(status) && status != "Tous")
            {
                initiatives = initiatives.Where(i => i.Status == status);
            }

            // Tri
            ViewData["TitleSortParm"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["VotesSortParm"] = sortOrder == "Votes" ? "votes_desc" : "Votes";

            initiatives = sortOrder switch
            {
                "title_desc" => initiatives.OrderByDescending(i => i.Title),
                "Date" => initiatives.OrderBy(i => i.CreatedAt),
                "date_desc" => initiatives.OrderByDescending(i => i.CreatedAt),
                "Votes" => initiatives.OrderBy(i => i.VotesCount),
                "votes_desc" => initiatives.OrderByDescending(i => i.VotesCount),
                _ => initiatives.OrderByDescending(i => i.CreatedAt),
            };

            var model = new InitiativeIndexViewModel
            {
                Initiatives = await initiatives.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                StatusList = new List<string> { "Tous", "En attente", "Approuvée", "En cours", "Terminée", "Rejetée" }
            };

            return View(model);
        }

        // GET: Initiative/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var initiative = await _context.Initiatives
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (initiative == null)
            {
                return NotFound();
            }

            var model = new InitiativeDetailsViewModel
            {
                Initiative = initiative,
                NewComment = new Comment(),
                CanEdit = User.Identity.IsAuthenticated && 
                         (User.IsInRole("Admin") || User.IsInRole("Moderator") || 
                          User.FindFirstValue(ClaimTypes.NameIdentifier) == initiative.UserId)
            };

            return View(model);
        }

        // GET: Initiative/Create
        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            
            // Debug: Vérifier si les catégories sont chargées
            var categories = ViewBag.Categories as List<Category>;
            if (categories == null || !categories.Any())
            {
                // Forcer la création des catégories si elles n'existent pas
                await ForceCreateCategories();
                await LoadCategories();
            }
            
            return View();
        }

        private async Task ForceCreateCategories()
        {
            var existingCategories = await _context.Categories.ToListAsync();
            if (!existingCategories.Any())
            {
                var defaultCategories = new[]
                {
                    new Category { Name = "Environnement", Description = "Initiatives pour la protection de l'environnement et la durabilité" },
                    new Category { Name = "Éducation", Description = "Projets éducatifs et de sensibilisation" },
                    new Category { Name = "Social", Description = "Actions sociales et communautaires" },
                    new Category { Name = "Culture", Description = "Initiatives culturelles et artistiques" },
                    new Category { Name = "Santé", Description = "Projets liés à la santé et au bien-être" },
                    new Category { Name = "Technologie", Description = "Innovations technologiques et numériques" },
                    new Category { Name = "Urbanisme", Description = "Aménagement urbain et projets de ville" },
                    new Category { Name = "Énergie", Description = "Projets liés aux énergies renouvelables et à l'efficacité énergétique" }
                };
                
                await _context.Categories.AddRangeAsync(defaultCategories);
                await _context.SaveChangesAsync();
            }
        }

        // POST: Initiative/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InitiativeCreateViewModel model)
        {
            // Débogage: afficher les données reçues
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"Title: {model.Title}");
            Console.WriteLine($"Description: {model.Description}");
            Console.WriteLine($"CategoryId: {model.CategoryId}");
            Console.WriteLine($"Location: {model.Location}");
            Console.WriteLine($"TargetDate: {model.TargetDate}");
            Console.WriteLine($"ImageFile: {model.ImageFile?.FileName}");

            // Afficher les erreurs de validation
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Erreur: {error.ErrorMessage}");
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError("", "Utilisateur non connecté.");
                    await LoadCategories();
                    return View(model);
                }

                var initiative = new Initiative
                {
                    Title = model.Title,
                    Description = model.Description,
                    Location = model.Location,
                    TargetDate = model.TargetDate,
                    CategoryId = model.CategoryId,
                    UserId = user.Id,
                    Status = "En attente",
                    CreatedAt = DateTime.UtcNow,
                    Duration = model.Duration ?? "Non spécifiée",
                    Goals = model.Goals ?? "",
                    RequiredResources = model.RequiredResources ?? "",
                    RequiredSkills = model.RequiredSkills ?? "",
                    Budget = model.Budget,
                    RejectionReason = null, // Explicitement null pour les nouvelles initiatives
                    ReviewedBy = null,
                    UpdatedAt = null
                };

                // Gestion du téléchargement d'image
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Validation du type de fichier
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("", "Seuls les fichiers JPG, PNG et GIF sont autorisés.");
                            await LoadCategories();
                            return View(model);
                        }

                        // Validation de la taille (max 5MB)
                        if (model.ImageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("", "La taille de l'image ne doit pas dépasser 5MB.");
                            await LoadCategories();
                            return View(model);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(model.ImageFile.FileName)}{fileExtension}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(fileStream);
                        }

                        initiative.ImageUrl = $"/uploads/{uniqueFileName}";
                        Console.WriteLine($"Image sauvegardée: {initiative.ImageUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de la sauvegarde de l'image: {ex.Message}");
                        ModelState.AddModelError("", "Erreur lors du téléchargement de l'image. Veuillez réessayer.");
                        await LoadCategories();
                        return View(model);
                    }
                }
                else
                {
                    // Pas d'image - utiliser une image par défaut ou laisser vide
                    initiative.ImageUrl = "";
                    Console.WriteLine("Aucune image fournie");
                }

                try
                {
                    Console.WriteLine("Tentative de sauvegarde de l'initiative...");
                    _context.Add(initiative);
                    
                    Console.WriteLine("Initiative ajoutée au contexte, tentative de sauvegarde dans la base de données...");
                    var result = await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"Sauvegarde réussie! {result} enregistrement(s) affecté(s).");
                    Console.WriteLine($"ID de l'initiative: {initiative.Id}");
                    Console.WriteLine($"Titre: {initiative.Title}");
                    Console.WriteLine($"Image URL: {initiative.ImageUrl}");
                    
                    // Envoyer une notification à l'utilisateur (après la sauvegarde)
                    try
                    {
                        await _notificationService.SendInitiativeSubmittedNotificationAsync(
                            user.Id, 
                            initiative.Id, 
                            initiative.Title
                        );
                        Console.WriteLine("Notification envoyée avec succès");
                    }
                    catch (Exception notifEx)
                    {
                        Console.WriteLine($"Erreur lors de l'envoi de la notification: {notifEx.Message}");
                        // Ne pas bloquer la création de l'initiative si la notification échoue
                    }
                    
                    TempData["SuccessMessage"] = "Votre initiative a été soumise pour approbation ! Vous recevrez une notification dès qu'elle sera examinée.";
                    return RedirectToAction("Index", "Initiative", new { area = "" });
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"=== ERREUR BASE DE DONNÉES DÉTAILLÉE ===");
                    Console.WriteLine($"Message principal: {dbEx.Message}");
                    Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");
                    Console.WriteLine($"Inner Exception Type: {dbEx.InnerException?.GetType().Name}");
                    
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Inner Exception: {dbEx.InnerException.InnerException?.Message}");
                    }
                    
                    Console.WriteLine($"Entity Entry: {dbEx.Entries?.FirstOrDefault()?.Entity?.GetType().Name}");
                    Console.WriteLine($"Stack Trace: {dbEx.StackTrace}");
                    
                    // Afficher les propriétés de l'initiative qui cause le problème
                    Console.WriteLine($"=== PROPERTIES DE L'INITIATIVE ===");
                    Console.WriteLine($"Title: '{initiative.Title}'");
                    Console.WriteLine($"Description: '{initiative.Description}'");
                    Console.WriteLine($"Location: '{initiative.Location}'");
                    Console.WriteLine($"CategoryId: {initiative.CategoryId}");
                    Console.WriteLine($"UserId: '{initiative.UserId}'");
                    Console.WriteLine($"Status: '{initiative.Status}'");
                    Console.WriteLine($"ImageUrl: '{initiative.ImageUrl}'");
                    Console.WriteLine($"CreatedAt: {initiative.CreatedAt}");
                    Console.WriteLine($"TargetDate: {initiative.TargetDate}");
                    Console.WriteLine($"Goals: '{initiative.Goals}'");
                    Console.WriteLine($"RequiredResources: '{initiative.RequiredResources}'");
                    Console.WriteLine($"Duration: '{initiative.Duration}'");
                    Console.WriteLine($"RequiredSkills: '{initiative.RequiredSkills}'");
                    Console.WriteLine($"Budget: {initiative.Budget}");
                    
                    ModelState.AddModelError("", $"Erreur base de données: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur générale: {ex.Message}");
                    Console.WriteLine($"Type: {ex.GetType().Name}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    ModelState.AddModelError($"", "Erreur technique: {ex.Message}");
                }
            }

            await LoadCategories();
            return View(model);
        }

        // GET: Initiative/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var initiative = await _context.Initiatives
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
            {
                return NotFound();
            }

            // Vérifier si l'utilisateur est le créateur ou un admin/modérateur
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (initiative.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            await LoadCategories();
            
            var model = new InitiativeCreateViewModel
            {
                Id = initiative.Id,
                Title = initiative.Title,
                Description = initiative.Description,
                CategoryId = initiative.CategoryId,
                TargetDate = initiative.TargetDate,
                Goals = initiative.Goals,
                RequiredResources = initiative.RequiredResources,
                Budget = initiative.Budget,
                Duration = initiative.Duration,
                RequiredSkills = initiative.RequiredSkills,
                Status = initiative.Status,
                ImageUrl = initiative.ImageUrl
            };

            return View(model);
        }

        // POST: Initiative/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InitiativeCreateViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var initiative = await _context.Initiatives.FindAsync(id);
                    if (initiative == null)
                    {
                        return NotFound();
                    }

                    // Vérifier les autorisations
                    var user = await _userManager.GetUserAsync(User);
                    if (!User.IsInRole("Admin") && !User.IsInRole("Moderator") && user.Id != initiative.UserId)
                    {
                        return Forbid();
                    }

                    // Mettre à jour les propriétés
                    initiative.Title = model.Title;
                    initiative.Description = model.Description;
                    initiative.Location = model.Location;
                    initiative.TargetDate = model.TargetDate;
                    initiative.CategoryId = model.CategoryId;
                    
                    // Seuls les administrateurs et modérateurs peuvent modifier le statut
                    if (User.IsInRole("Admin") || User.IsInRole("Moderator"))
                    {
                        initiative.Status = model.Status;
                    }

                    // Gestion de l'image
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        // Supprimer l'ancienne image si elle existe
                        if (!string.IsNullOrEmpty(initiative.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, initiative.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Télécharger la nouvelle image
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                        var uniqueFileName = $"{Guid.NewGuid()}_{model.ImageFile.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(fileStream);
                        }

                        initiative.ImageUrl = $"/uploads/{uniqueFileName}";
                    }

                    _context.Update(initiative);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "L'initiative a été mise à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InitiativeExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Initiative", new { area = "", id = model.Id });
            }

            await LoadCategories();
            return View(model);
        }

        // GET: Initiative/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var initiative = await _context.Initiatives
                .Include(i => i.User)
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
            {
                return NotFound();
            }

            // Vérifier si l'utilisateur est le créateur ou un admin/modérateur
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (initiative.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            return View(initiative);
        }

        // POST: Initiative/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var initiative = await _context.Initiatives
                .Include(i => i.Comments)
                .Include(i => i.Votes)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
            {
                return NotFound();
            }

            // Vérifier les autorisations
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && !User.IsInRole("Moderator") && initiative.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                // Supprimer l'image associée si elle existe
                if (!string.IsNullOrEmpty(initiative.ImageUrl))
                {
                    var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, initiative.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Initiatives.Remove(initiative);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "L'initiative a été supprimée avec succès.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de l'initiative.";
                // Log l'erreur pour le débogage
                Console.WriteLine($"Error deleting initiative: {ex.Message}");
            }

            return RedirectToAction("Index", "Initiative", new { area = "" });
        }

        // POST: Initiative/Vote/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int id, bool isUpvote)
        {
            var userId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vous devez être connecté pour voter." });
            }
            
            var initiative = await _context.Initiatives.FindAsync(id);
            
            if (initiative == null)
            {
                return Json(new { success = false, message = "Initiative non trouvée." });
            }

            // Vérifier si l'utilisateur a déjà voté pour cette initiative
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.InitiativeId == id && v.UserId == userId);

            if (existingVote != null)
            {
                // Si l'utilisateur clique sur le même type de vote, on le supprime
                if (existingVote.IsUpvote == isUpvote)
                {
                    _context.Votes.Remove(existingVote);
                    initiative.VotesCount += isUpvote ? -1 : 1;
                }
                // Sinon, on inverse le vote
                else
                {
                    existingVote.IsUpvote = isUpvote;
                    initiative.VotesCount += isUpvote ? 2 : -2; // +1 pour le nouveau vote, -1 pour l'ancien
                }
            }
            else
            {
                // Nouveau vote
                var vote = new Vote
                {
                    InitiativeId = id,
                    UserId = userId,
                    IsUpvote = isUpvote,
                    UserInitiativeKey = $"{userId}_{id}" // Clé unique pour éviter les doublons
                };
                
                _context.Votes.Add(vote);
                initiative.VotesCount += isUpvote ? 1 : -1;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, newVoteCount = initiative.VotesCount });
        }

        // POST: Initiative/AddComment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Le commentaire ne peut pas être vide.");
            }

            var userId = _userManager.GetUserId(User);
            var initiative = await _context.Initiatives.FindAsync(id);
            
            if (initiative == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Content = content.Trim(),
                InitiativeId = id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Recharger l'utilisateur pour inclure les informations nécessaires à l'affichage
            comment.User = await _userManager.FindByIdAsync(userId);

            return Json(new { 
                success = true, 
                comment = new {
                    id = comment.Id,
                    content = comment.Content,
                    createdAt = comment.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    userName = comment.User.UserName
                }
            });
        }

        private bool InitiativeExists(int id)
        {
            return _context.Initiatives.Any(e => e.Id == id);
        }

        private async Task LoadCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            
            // Si aucune catégorie n'existe, en créer des catégories par défaut
            if (!categories.Any())
            {
                var defaultCategories = new[]
                {
                    new Category { Name = "Environnement", Description = "Initiatives pour la protection de l'environnement et la durabilité" },
                    new Category { Name = "Éducation", Description = "Projets éducatifs et de sensibilisation" },
                    new Category { Name = "Social", Description = "Actions sociales et communautaires" },
                    new Category { Name = "Culture", Description = "Initiatives culturelles et artistiques" },
                    new Category { Name = "Santé", Description = "Projets liés à la santé et au bien-être" },
                    new Category { Name = "Technologie", Description = "Innovations technologiques et numériques" },
                    new Category { Name = "Urbanisme", Description = "Aménagement urbain et projets de ville" },
                    new Category { Name = "Énergie", Description = "Projets liés aux énergies renouvelables et à l'efficacité énergétique" }
                };
                
                await _context.Categories.AddRangeAsync(defaultCategories);
                await _context.SaveChangesAsync();
                
                categories = await _context.Categories.ToListAsync();
            }
            
            ViewBag.Categories = categories;
        }
    }
}
