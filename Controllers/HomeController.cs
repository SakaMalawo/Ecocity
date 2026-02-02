using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcoCity.Models;
using Microsoft.EntityFrameworkCore;
using EcoCity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcoCity.ViewModels;

namespace EcoCity.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger, 
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            // Récupérer les initiatives les plus récentes
            RecentInitiatives = await _context.Initiatives
                .Include(i => i.User)
                .Include(i => i.Category)
                .OrderByDescending(i => i.CreatedAt)
                .Take(6)
                .ToListAsync(),
                
            // Récupérer les initiatives les plus populaires (celles avec le plus de votes)
            PopularInitiatives = await _context.Initiatives
                .Include(i => i.User)
                .Include(i => i.Category)
                .OrderByDescending(i => i.VotesCount)
                .Take(6)
                .ToListAsync(),
                
            // Statistiques générales
            TotalInitiatives = await _context.Initiatives.CountAsync(),
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalVotes = await _context.Votes.CountAsync(),
            TotalComments = await _context.Comments.CountAsync()
        };

        return View(model);
    }

    [Authorize]
    public IActionResult Dashboard()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        else if (User.IsInRole("Moderator"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Moderator" });
        }
        
        // Pour les utilisateurs normaux, rediriger vers leur profil
        return RedirectToAction("Profile", "Account");
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Ici, vous pourriez ajouter la logique pour envoyer un email
            // Par exemple, en utilisant un service d'envoi d'emails
            
            TempData["Message"] = "Votre message a été envoyé avec succès. Nous vous répondrons dès que possible.";
            return RedirectToAction("Contact");
        }
        
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    [Route("error/{statusCode}")]
    public IActionResult Error(int statusCode)
    {
        var errorModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode
        };
        
        switch (statusCode)
        {
            case 404:
                errorModel.ErrorMessage = "La page que vous recherchez n'existe pas.";
                break;
            case 403:
                errorModel.ErrorMessage = "Vous n'êtes pas autorisé à accéder à cette ressource.";
                break;
            case 500:
                errorModel.ErrorMessage = "Une erreur interne du serveur s'est produite.";
                break;
            default:
                errorModel.ErrorMessage = "Une erreur inattendue s'est produite.";
                break;
        }
        
        return View("Error", errorModel);
    }
}
