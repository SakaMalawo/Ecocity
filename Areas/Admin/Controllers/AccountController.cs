using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EcoCity.Models;
using EcoCity.Models.AccountViewModels;
using EcoCity.Services;

namespace EcoCity.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IAdminService _adminService;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IAdminService adminService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _adminService = adminService;
            _configuration = configuration;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            _logger.LogInformation("Tentative de login admin: Email={Email}", model.Email);

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    _logger.LogInformation("Utilisateur trouvé: UserId={UserId}, UserName={UserName}", user.Id, user.UserName);
                    
                    // Vérifier si l'utilisateur est un admin
                    var isAdmin = await _adminService.IsUserAdminAsync(user.Id);
                    _logger.LogInformation("Vérification admin: IsAdmin={IsAdmin}", isAdmin);
                    
                    if (!isAdmin)
                    {
                        _logger.LogWarning("Accès refusé pour {Email}: pas admin", model.Email);
                        ViewData["ErrorMessage"] = "Accès refusé. Vous n'avez pas les droits d'administrateur.";
                        return View(model);
                    }

                    // Vérifier le mot de passe
                    var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
                    _logger.LogInformation("Vérification mot de passe: PasswordValid={PasswordValid}", passwordCheck);
                    
                    if (!passwordCheck)
                    {
                        _logger.LogWarning("Mot de passe incorrect pour {Email}", model.Email);
                        ViewData["ErrorMessage"] = "Email ou mot de passe incorrect.";
                        return View(model);
                    }

                    // Sign in
                    await _signInManager.SignOutAsync();
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                    _logger.LogInformation("Résultat PasswordSignIn: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, IsNotAllowed={IsNotAllowed}", 
                        result.Succeeded, result.IsLockedOut, result.IsNotAllowed);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Admin {Email} connecté avec succès", user.Email);
                        
                        // Update last login
                        var admin = await _adminService.GetAdminByUserIdAsync(user.Id);
                        if (admin != null)
                        {
                            admin.LastLoginAt = DateTime.UtcNow;
                            await _adminService.UpdateAdminAsync(admin);
                        }

                        // Force refresh sign in to ensure roles are loaded
                        await _signInManager.RefreshSignInAsync(user);

                        _logger.LogInformation("Redirection vers Admin/Dashboard/Index");
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("Compte verrouillé pour {Email}", model.Email);
                        ViewData["ErrorMessage"] = "Votre compte est verrouillé.";
                        return View(model);
                    }

                    if (result.IsNotAllowed)
                    {
                        _logger.LogWarning("Compte non activé pour {Email}", model.Email);
                        ViewData["ErrorMessage"] = "Votre compte n'est pas activé.";
                        return View(model);
                    }
                }
                else
                {
                    _logger.LogWarning("Utilisateur non trouvé: {Email}", model.Email);
                    ViewData["ErrorMessage"] = "Email ou mot de passe incorrect.";
                    return View(model);
                }
            }
            else
            {
                _logger.LogWarning("ModelState invalide pour {Email}", model.Email);
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Erreur de validation: {Error}", modelError.ErrorMessage);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Admin déconnecté");
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Admin déconnecté");
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        // Endpoint API pour créer un admin (protégé par clé secrète)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegistrationRequest request)
        {
            // Vérifier la clé secrète depuis la configuration
            var secretKey = _configuration["AdminSettings:SecretKey"];
            if (string.IsNullOrEmpty(secretKey) || request.SecretKey != secretKey)
            {
                return Unauthorized(new { success = false, message = "Clé secrète invalide" });
            }

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { success = false, message = "Email et mot de passe requis" });
            }

            // Vérifier si l'utilisateur existe déjà
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                // Promouvoir l'utilisateur existant en admin s'il ne l'est pas déjà
                var isAdmin = await _adminService.IsUserAdminAsync(existingUser.Id);
                if (!isAdmin)
                {
                    await _userManager.AddToRoleAsync(existingUser, "Admin");
                    await _adminService.CreateAdminAsync(existingUser.Id, request.Role ?? "Admin", "API");
                    return Ok(new { success = true, message = "Utilisateur promu administrateur" });
                }
                return Ok(new { success = true, message = "L'utilisateur est déjà administrateur" });
            }

            // Créer le nouvel utilisateur
            var user = new ApplicationUser
            {
                UserName = request.UserName ?? request.Email,
                Email = request.Email,
                FirstName = request.FirstName ?? "Admin",
                LastName = request.LastName ?? "User",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            // Ajouter le rôle Identity
            await _userManager.AddToRoleAsync(user, request.Role ?? "Admin");

            // Créer l'entrée dans la table Admin
            await _adminService.CreateAdminAsync(user.Id, request.Role ?? "Admin", "API");

            _logger.LogInformation("Admin créé via API: {Email}", request.Email);

            return Ok(new { success = true, message = "Administrateur créé avec succès" });
        }

        // Endpoint pour vérifier si un utilisateur est admin
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckAdminStatus()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var isAdmin = await _adminService.IsUserAdminAsync(userId);
            return Ok(new { isAdmin });
        }

        // Endpoint de secours pour créer l'entrée admin si l'utilisateur a le rôle Identity Admin
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> FixAdminEntry([FromBody] FixAdminRequest request)
        {
            // Vérifier la clé secrète
            var secretKey = _configuration["AdminSettings:SecretKey"];
            if (string.IsNullOrEmpty(secretKey) || request.SecretKey != secretKey)
            {
                return Unauthorized(new { success = false, message = "Clé secrète invalide" });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Utilisateur non trouvé" });
            }

            // Vérifier si déjà admin dans la table
            var isAlreadyAdmin = await _adminService.IsUserAdminAsync(user.Id);
            if (isAlreadyAdmin)
            {
                return Ok(new { success = true, message = "L'utilisateur est déjà administrateur" });
            }

            // Vérifier si l'utilisateur a le rôle Identity "Admin"
            var hasAdminRole = await _userManager.IsInRoleAsync(user, "Admin");
            if (!hasAdminRole)
            {
                // Ajouter le rôle s'il ne l'a pas
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            // Créer l'entrée dans la table Admin
            await _adminService.CreateAdminAsync(user.Id, "Admin", "FixAdminEndpoint");

            _logger.LogInformation("Admin entry created for {Email} via FixAdminEndpoint", request.Email);

            return Ok(new { 
                success = true, 
                message = "Entrée administrateur créée avec succès",
                userId = user.Id,
                email = user.Email
            });
        }
    }

    public class FixAdminRequest
    {
        public string SecretKey { get; set; }
        public string Email { get; set; }
    }

    public class AdminRegistrationRequest
    {
        public string SecretKey { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }
}
