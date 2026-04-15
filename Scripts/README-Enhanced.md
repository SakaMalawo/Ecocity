# Système d'Administration Amélioré - EcoCity

Ce dossier contient des scripts améliorés pour créer des administrateurs avec un système complet de gestion incluant :

## Nouvelles Fonctionnalités

### 1. **Table Admin Dédiée**
- Suivi des administrateurs dans une table dédiée
- Statistiques individuelles (initiatives approuvées/rejetées)
- Informations supplémentaires (département, notes, permissions)
- Historique des actions

### 2. **Système de Notifications**
- Notifications automatiques pour les déposants
- Alertes en temps réel pour les administrateurs
- Suivi du statut des initiatives (en attente/approuvée/rejetée)
- Motifs de rejet communiqués aux utilisateurs

### 3. **Dashboard Admin Intelligent**
- Statistiques en temps réel
- Taux d'approbation et temps moyen de réponse
- Top initiatives et utilisateurs actifs
- Interface moderne avec glassmorphism

## Scripts Disponibles

### 1. PowerShell (Windows) - Version Améliorée
- **Fichier**: `Create-Admin-Enhanced.ps1`
- **Usage**: Exécutez dans PowerShell avec `.\Create-Admin-Enhanced.ps1`

### 2. Bash (Linux/macOS) - Version Améliorée
- **Fichier**: `create-admin-enhanced.sh`
- **Usage**: Exécutez dans le terminal avec `./create-admin-enhanced.sh`

## Étapes d'Installation

### 1. Préparation de la Base de Données

```bash
# Appliquer les migrations (inclut les tables Notifications et Admins)
dotnet ef database update
```

### 2. Exécution du Script Amélioré

#### Sur Windows (PowerShell):

```powershell
# Naviguer vers le répertoire du projet
cd C:\Users\sakam\Documents\DOTNET\Ecocity

# Exécuter le script amélioré
.\Scripts\Create-Admin-Enhanced.ps1
```

#### Sur Linux/macOS:

```bash
# Naviguer vers le répertoire du projet
cd /path/to/EcoCity

# Rendre le script exécutable
chmod +x Scripts/create-admin-enhanced.sh

# Exécuter le script amélioré
./Scripts/create-admin-enhanced.sh
```

### 3. Utilisation du Code Généré

Les scripts génèrent deux fichiers :

1. **CreateAdmin-Enhanced-Code.cs** - Code C# à ajouter dans Program.cs
2. **CreateAdmin-Enhanced-SQL.sql** - Script SQL de secours

#### Ajout dans Program.cs

```csharp
// Ajoutez ce code avant 'app.Run()' dans Program.cs

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var adminService = services.GetRequiredService<IAdminService>();
    
    // Créer le rôle s'il n'existe pas
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    // Créer l'utilisateur administrateur
    var user = new ApplicationUser
    {
        UserName = "admin",
        Email = "admin@ecocity.com",
        FirstName = "Admin",
        LastName = "User",
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
    
    var result = await userManager.CreateAsync(user, "YourPassword123!");
    
    if (result.Succeeded)
    {
        // Ajouter le rôle Identity
        await userManager.AddToRoleAsync(user, "Admin");
        
        // Créer l'entrée dans la table Admin
        await adminService.CreateAdminAsync(user.Id, "Admin", "System");
        
        Console.WriteLine("Administrateur créé avec succès!");
    }
}
```

## Workflow d'Approbation

### 1. Soumission d'Initiative
- L'utilisateur crée une initiative
- Statut automatique: "En attente"
- Notification envoyée: "Initiative soumise pour approbation"

### 2. Review par l'Admin
- L'admin voit les initiatives en attente dans le dashboard
- Possibilité d'approuver ou de rejeter avec motif
- Statistiques de l'admin mises à jour automatiquement

### 3. Notification du Déposant
- **Approbation**: "Félicitations ! Votre initiative a été approuvée"
- **Rejet**: "Votre initiative n'a pas pu être approuvée. Raison: [motif]"

## Accès Admin

### URLs Disponibles
- **Connexion**: `https://localhost:5001/admin/login`
- **Dashboard**: `https://localhost:5001/admin/dashboard`
- **Initiatives en attente**: `https://localhost:5001/admin/initiatives/pending`

### Rôles et Permissions

#### Admin
- Accès complet au dashboard
- Approuver/rejeter les initiatives
- Gérer les administrateurs
- Accès aux rapports et statistiques complètes

#### Moderator
- Approuver/rejeter les initiatives
- Accès limité aux statistiques
- Pas d'accès à la gestion des administrateurs

## Structure des Tables

### Table Admins
```sql
CREATE TABLE Admins (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId NVARCHAR(450) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    Department NVARCHAR(100),
    Permissions NVARCHAR(MAX),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    LastLoginAt DATETIME2,
    CreatedBy NVARCHAR(450),
    Notes NVARCHAR(500),
    InitiativesApproved INT DEFAULT 0,
    InitiativesRejected INT DEFAULT 0,
    ActionsCount INT DEFAULT 0,
    LastActionAt DATETIME2
);
```

### Table Notifications
```sql
CREATE TABLE Notifications (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId NVARCHAR(450) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    RelatedEntityType NVARCHAR(50),
    RelatedEntityId INT,
    ActionUrl NVARCHAR(500),
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    ReadAt DATETIME2
);
```

## Dépannage

### Erreur: "L'utilisateur existe déjà"
- Le script gère automatiquement ce cas
- Si l'utilisateur existe mais n'est pas admin, il sera promu
- Sinon, un message indiquera qu'il est déjà admin

### Erreur: "Accès non autorisé"
- Vérifiez que l'utilisateur a bien le rôle dans Identity ET dans la table Admin
- Redémarrez l'application après avoir ajouté le rôle

### Erreur: "Base de données non mise à jour"
- Assurez-vous d'avoir appliqué toutes les migrations
- Vérifiez que les tables Notifications et Admins existent

## Monitoring et Statistiques

### Dashboard Admin
- **Taux d'approbation**: Pourcentage d'initiatives approuvées
- **Temps moyen de réponse**: Temps entre soumission et review
- **Statistiques par admin**: Actions individuelles de chaque administrateur
- **Tendances mensuelles**: Évolution des soumissions et approbations

### Notifications
- **Non lues**: Compteur en temps réel dans l'interface
- **Historique**: Toutes les notifications avec statut lu/non lu
- **Actions directes**: Liens vers les initiatives concernées

## Support

Pour toute question sur le système d'administration amélioré, contactez l'équipe de développement EcoCity.
