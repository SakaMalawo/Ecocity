using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using EcoCity.Models;
using EcoCity.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text;
using System.Text.Encodings.Web;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EcoCity.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            IUserStore<ApplicationUser> userStore,
            IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            _hostingEnvironment = hostingEnvironment;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Vider le cookie externe existant pour assurer un processus de connexion propre
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Vérifier si l'utilisateur a un mot de passe
                    if (string.IsNullOrEmpty(user.PasswordHash))
                    {
                        ViewData["ErrorMessage"] = "Votre compte n'a pas de mot de passe défini. Veuillez vous réinscrire.";
                        return View(model);
                    }
                    
                    // Vérifier le mot de passe
                    var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (!passwordCheck)
                    {
                        ViewData["ErrorMessage"] = "Email ou mot de passe incorrect. Veuillez vérifier vos informations et réessayer.";
                        return View(model);
                    }
                    
                    // Nettoyer les sessions existantes et tenter la connexion avec le UserName
                    await _signInManager.SignOutAsync();
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                    
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Connexion réussie ! Bienvenue sur EcoCity.";
                        return RedirectToLocal(returnUrl);
                    }
                    
                    if (result.IsLockedOut)
                    {
                        ViewData["ErrorMessage"] = "Votre compte est temporairement verrouillé après plusieurs tentatives échouées. Veuillez réessayer plus tard.";
                        return View(model);
                    }
                    
                    if (result.IsNotAllowed)
                    {
                        ViewData["ErrorMessage"] = "Votre compte n'est pas encore activé. Veuillez vérifier votre email.";
                        return View(model);
                    }
                    
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                    }
                }
                else
                {
                    ViewData["ErrorMessage"] = "Email ou mot de passe incorrect. Veuillez vérifier vos informations et réessayer.";
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Vérifier que l'utilisateur a déjà une session
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException("Impossible de charger l'utilisateur pour l'authentification à deux facteurs.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Impossible de charger l'utilisateur avec l'ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("Utilisateur avec l'ID {UserId} s'est connecté avec 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("Compte utilisateur verrouillé.");
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Code d'authentification non valide saisi pour l'utilisateur avec l'ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Code d'authentification non valide.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Vérifier que l'utilisateur a déjà une session
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException("Impossible de charger l'utilisateur pour la récupération à deux facteurs.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException("Impossible de charger l'utilisateur pour la récupération à deux facteurs.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("Utilisateur avec l'ID {UserId} s'est connecté avec un code de récupération.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Compte utilisateur verrouillé.");
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Code de récupération non valide saisi pour l'utilisateur avec l'ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Code de récupération non valide saisi.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            _logger.LogInformation("Register POST method called - Model received: {@Model}", model);
            ViewData["ReturnUrl"] = returnUrl;
            
            // Test de connexion à la base de données
            try
            {
                var userCount = _userManager.Users.Count();
                _logger.LogInformation("Current users count in database: {Count}", userCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                ViewData["ErrorMessage"] = "Erreur de connexion à la base de données.";
                return View(model);
            }
            
            // Logging pour déboguer
            _logger.LogInformation("Register attempt - AcceptTerms: {AcceptTerms}, Email: {Email}, FirstName: {FirstName}", 
                model.AcceptTerms, model.Email, model.FirstName);
            
            // Forcer la validation manuelle du checkbox
            if (!model.AcceptTerms)
            {
                _logger.LogWarning("AcceptTerms is false - showing error");
                ModelState.AddModelError("AcceptTerms", "Vous devez accepter les conditions d'utilisation.");
                ViewData["ErrorMessage"] = "Vous devez accepter les conditions d'utilisation pour continuer.";
                return View(model);
            }
            
            // Si AcceptTerms est true, on supprime l'erreur de validation s'il y en a une
            if (model.AcceptTerms && ModelState.ContainsKey("AcceptTerms"))
            {
                ModelState.Remove("AcceptTerms");
            }
            
            _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.UserName ?? model.Email, 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address ?? null,
                    City = model.City ?? null,
                    PostalCode = model.PostalCode ?? null,
                    Bio = model.Bio ?? null,
                    Location = model.Location ?? null,
                    ProfilePicture = "",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("L'utilisateur a créé un nouveau compte avec mot de passe.");

                    // Par défaut, tous les nouveaux utilisateurs ont le rôle "User"
                    await _userManager.AddToRoleAsync(user, "User");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "Confirmez votre email",
                        $"Veuillez confirmer votre compte en <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>cliquant ici</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        TempData["SuccessMessage"] = "Compte créé avec succès ! Veuillez vérifier votre email pour confirmer votre inscription.";
                        return RedirectToAction("RegisterConfirmation", new { email = model.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        TempData["SuccessMessage"] = $"Bienvenue {user.FirstName} ! Votre compte a été créé avec succès.";
                        return RedirectToLocal(returnUrl);
                    }
                }
                else
                {
                    // Ajouter un message d'erreur personnalisé
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    var errorMessage = errors.FirstOrDefault() ?? "Une erreur est survenue lors de la création du compte.";
                    
                    if (errors.Any(e => e.Contains("Password")))
                    {
                        errorMessage = "Le mot de passe ne respecte pas les critères de sécurité requis (8 caractères minimum, une majuscule, une minuscule, un chiffre et un caractère spécial).";
                    }
                    else if (errors.Any(e => e.Contains("Email")))
                    {
                        errorMessage = "Cet adresse email est déjà utilisée. Veuillez vous connecter ou utiliser une autre adresse email.";
                    }
                    else if (errors.Any(e => e.Contains("UserName")))
                    {
                        errorMessage = "Ce nom d'utilisateur est déjà pris. Veuillez en choisir un autre.";
                    }
                    
                    ViewData["ErrorMessage"] = errorMessage;
                    _logger.LogWarning("Échec de la création du compte: {Errors}", string.Join(", ", errors));
                }
            }
            else
            {
                // Logging des erreurs de ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState errors: {Errors}", string.Join(", ", errors));
            }

            // Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("L'utilisateur s'est déconnecté.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("L'utilisateur s'est déconnecté.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Demander une redirection vers le fournisseur de connexion externe
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Erreur des services externes: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Connecter l'utilisateur avec ce fournisseur de connexion externe si l'utilisateur a déjà une connexion
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("L'utilisateur s'est connecté avec {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // Si l'utilisateur n'a pas de compte, alors demandez à l'utilisateur de créer un compte
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name)?.Split(' ');
                return View("ExternalLogin", new ExternalLoginViewModel
                {
                    Email = email,
                    FirstName = name != null && name.Length > 0 ? name[0] : "",
                    LastName = name != null && name.Length > 1 ? name[1] : ""
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Obtenir les informations sur l'utilisateur à partir du fournisseur de connexion externe
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Erreur lors du chargement des informations de connexion externe pendant la confirmation.");
                }

                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    EmailConfirmed = true // Les fournisseurs externes confirment généralement l'email
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        // Par défaut, tous les nouveaux utilisateurs ont le rôle "User"
                        await _userManager.AddToRoleAsync(user, "User");
                        
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("L'utilisateur a créé un compte en utilisant {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string returnUrl = null)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur avec l'ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                // Si l'email est confirmé avec succès, connectez automatiquement l'utilisateur
                await _signInManager.SignInAsync(user, isPersistent: false);
                StatusMessage = "Merci d'avoir confirmé votre email.";
                return RedirectToLocal(returnUrl);
            }
            
            return View("Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Ne pas révéler que l'utilisateur n'existe pas ou n'est pas confirmé
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // Pour plus d'informations sur la façon d'activer la confirmation de compte et la réinitialisation de mot de passe, 
                // veuillez visiter https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Réinitialiser le mot de passe",
                    $"Veuillez réinitialiser votre mot de passe en <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>cliquant ici</a>.");

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return BadRequest("Un code doit être fourni pour la réinitialisation du mot de passe.");
            }
            else
            {
                var model = new ResetPasswordViewModel { Code = code };
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Ne pas révéler que l'utilisateur n'existe pas
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur avec l'ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ProfileViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                PostalCode = user.PostalCode,
                ProfileImageUrl = user.ProfileImageUrl,
                IsEmailConfirmed = user.EmailConfirmed
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur avec l'ID '{_userManager.GetUserId(User)}'.");
            }

            // Mettre à jour les propriétés de l'utilisateur
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.PostalCode = model.PostalCode;

            // Gestion du téléchargement de l'image de profil
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Supprimer l'ancienne image si elle existe
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{model.ProfileImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                user.ProfileImageUrl = $"/uploads/profiles/{uniqueFileName}";
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            StatusMessage = "Votre profil a été mis à jour";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur avec l'ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("L'utilisateur a modifié son mot de passe avec succès.");
            TempData["SuccessMessage"] = "Votre mot de passe a été modifié avec succès.";

            return RedirectToAction(nameof(Profile));
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
