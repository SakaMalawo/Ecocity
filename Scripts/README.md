# Scripts de Création d'Administrateur EcoCity

Ce dossier contient des scripts interactifs pour créer des administrateurs pour la plateforme EcoCity.

## Scripts Disponibles

### 1. PowerShell (Windows)
- **Fichier**: `Create-Admin.ps1`
- **Usage**: Exécutez dans PowerShell avec `.\Create-Admin.ps1`

### 2. Bash (Linux/macOS)
- **Fichier**: `create-admin.sh`
- **Usage**: Exécutez dans le terminal avec `./create-admin.sh`

## Prérequis

1. **.NET SDK** installé (version 6.0 ou supérieure)
2. **Base de données** configurée et migrations appliquées
3. **Projet EcoCity** accessible

## Étapes d'Installation

### 1. Préparation de la Base de Données

```bash
# Appliquer les migrations
dotnet ef database update

# Créer les rôles par défaut (si nécessaire)
dotnet run --seed-roles
```

### 2. Exécution du Script

#### Sur Windows (PowerShell):

```powershell
# Naviguer vers le répertoire du projet
cd C:\Users\sakam\Documents\DOTNET\Ecocity

# Exécuter le script
.\Scripts\Create-Admin.ps1
```

#### Sur Linux/macOS:

```bash
# Naviguer vers le répertoire du projet
cd /path/to/EcoCity

# Rendre le script exécutable
chmod +x Scripts/create-admin.sh

# Exécuter le script
./Scripts/create-admin.sh
```

### 3. Utilisation du Code Généré

Les scripts génèrent deux fichiers :

1. **temp-admin-creation.sql** - Script SQL pour la base de données
2. **CreateAdmin-Code.cs** - Code C# à ajouter dans Program.cs

#### Option A: Via le code C# (Recommandé)

Ajoutez le code de `CreateAdmin-Code.cs` dans votre fichier `Program.cs` avant `app.Run()`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Créer le rôle s'il n'existe pas
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    // Créer l'utilisateur
    var user = new ApplicationUser
    {
        UserName = "admin",
        Email = "admin@ecocity.com",
        FirstName = "Admin",
        LastName = "User",
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    
    var result = await userManager.CreateAsync(user, "YourPassword123!");
    
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, "Admin");
        Console.WriteLine("Administrateur créé avec succès!");
    }
    else
    {
        Console.WriteLine("Erreur lors de la création: " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
```

#### Option B: Via SQL Direct

Exécutez le script SQL généré sur votre base de données.

## Accès Admin

Une fois l'administrateur créé :

1. **URL de connexion**: `https://localhost:5001/admin/login`
2. **URL du dashboard**: `https://localhost:5001/admin/dashboard`

## Rôles Disponibles

### Admin
- Accès complet au dashboard
- Approuver/rejeter les initiatives
- Gérer les utilisateurs
- Accès aux rapports et statistiques

### Moderator
- Approuver/rejeter les initiatives
- Accès limité aux statistiques
- Pas d'accès à la gestion des utilisateurs

## Sécurité

- Les mots de passe doivent contenir au moins 8 caractères
- Les emails doivent être valides
- Les administrateurs doivent avoir un rôle valide pour accéder au dashboard
- La connexion admin utilise une page dédiée avec authentification renforcée

## Dépannage

### Erreur: "L'utilisateur existe déjà"
- Vérifiez si l'email ou le nom d'utilisateur est déjà utilisé
- Utilisez un email différent ou supprimez l'utilisateur existant

### Erreur: "Accès non autorisé"
- Vérifiez que l'utilisateur a bien le rôle "Admin" ou "Moderator"
- Redémarrez l'application après avoir ajouté le rôle

### Erreur: "Base de données non trouvée"
- Assurez-vous que la base de données est bien configurée
- Appliquez les migrations avec `dotnet ef database update`

## Support

Pour toute question ou problème, contactez l'équipe de développement EcoCity.
