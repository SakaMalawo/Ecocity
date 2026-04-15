using EcoCity.Data;
using EcoCity.Models;
using EcoCity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour la journalisation
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/ecocity-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration des services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configuration d'Entity Framework Core avec MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configuration d'Identity avec des options personnalisées
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configuration du service d'email
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EcoCity.Services.NoOpEmailSender>();

// Configuration des cookies d'authentification
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Ajout des services de contrôleurs avec vues
builder.Services.AddControllersWithViews();

// Configuration de la politique d'autorisation pour les rôles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireModeratorRole", policy => policy.RequireRole("Moderator"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});

// Configuration des services personnalisés
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Configuration de Swagger pour la documentation de l'API
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuration du pipeline de requêtes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoCity API v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware pour la gestion des erreurs
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Middleware pour le traitement des requêtes HTTPS
app.UseHttpsRedirection();

// Middleware pour les fichiers statiques (wwwroot)
app.UseStaticFiles();

// Middleware pour le routage
app.UseRouting();

// Middleware d'authentification et d'autorisation
app.UseAuthentication();
app.UseAuthorization();

// Configuration des routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialisation de la base de données avec des données de test
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Appliquer les migrations
        await context.Database.MigrateAsync();
        
        // Initialiser les rôles et l'utilisateur admin
        try
        {
            await ApplicationDbInitializer.InitializeAsync(userManager, roleManager, services);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Erreur lors de l'initialisation de la base de données, mais l'application continue de fonctionner.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Une erreur est survenue lors de l'initialisation de la base de données.");
    }
}

app.Run();
