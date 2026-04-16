# Documentation - Authentification Admin EcoCity

## Vue d'ensemble

Ce document décrit le système d'authentification des administrateurs pour EcoCity, incluant les endpoints, les scripts de création d'admin, et la configuration.

## Comptes Admin par défaut

L'application initialise automatiquement les comptes admin suivants lors du premier démarrage :

| Email | Mot de passe | Rôle |
|-------|--------------|------|
| admin@ecocity.com | Admin@123 | Admin |
| moderator@ecocity.com | Moderator@123 | Moderator |

⚠️ **Important** : Changez ces mots de passe en production !

## Configuration

### appsettings.json

```json
{
  "AdminSettings": {
    "SecretKey": "EcoCity-Admin-Secret-Key-2024-Change-In-Production",
    "DefaultAdminEmail": "admin@ecocity.com",
    "DefaultAdminPassword": "Admin@123"
  }
}
```

**Important** : Changez `SecretKey` en production pour sécuriser l'endpoint d'inscription d'admin.

## Endpoints Admin

### 1. Login Admin

**URL** : `POST /Admin/Account/Login`

**Vue** : `Areas/Admin/Views/Account/Login.cshtml`

**Paramètres** :
- `Email` : Email de l'admin
- `Password` : Mot de passe
- `RememberMe` : Optionnel, garder la session active
- `ReturnUrl` : Optionnel, URL de retour après connexion

**Vérifications** :
- Vérifie si l'utilisateur existe
- Vérifie si l'utilisateur est admin (table `Admins`)
- Vérifie le mot de passe
- Met à jour `LastLoginAt` dans la table Admin

**Redirection** : Après connexion réussie, redirige vers `/Admin/Dashboard`

### 2. Logout Admin

**URL** : `GET /Admin/Account/Logout` ou `POST /Admin/Account/LogoutPost`

Redirige vers la page de login admin.

### 3. Inscription Admin via API

**URL** : `POST /Admin/Account/RegisterAdmin`

**Headers** :
- `Content-Type: application/json`

**Body** :
```json
{
  "SecretKey": "EcoCity-Admin-Secret-Key-2024-Change-In-Production",
  "Email": "newadmin@ecocity.com",
  "Password": "SecurePassword123!",
  "UserName": "newadmin",
  "FirstName": "New",
  "LastName": "Admin",
  "Role": "Admin"
}
```

**Réponse** :
```json
{
  "success": true,
  "message": "Administrateur créé avec succès"
}
```

**Comportement** :
- Vérifie la clé secrète depuis `appsettings.json`
- Si l'utilisateur existe déjà, le promeut en admin
- Sinon, crée un nouvel utilisateur avec le rôle spécifié
- Ajoute l'entrée dans la table `Admins`

### 4. Vérifier Status Admin

**URL** : `GET /Admin/Account/CheckAdminStatus`

**Authentification** : Requiert être connecté

**Réponse** :
```json
{
  "isAdmin": true
}
```

## Scripts de Création d'Admin

### PowerShell (Windows)

**Fichier** : `Scripts/Create-Admin-Enhanced.ps1`

**Utilisation** :
```powershell
cd C:\Users\sakam\Documents\DOTNET\Ecocity
.\Scripts\Create-Admin-Enhanced.ps1
```

Le script génère :
- Un fichier C# avec le code d'initialisation
- Un script SQL pour la table Admin
- Instructions pour l'intégration

### Bash (Linux/Mac)

**Fichier** : `Scripts/create-admin-enhanced.sh`

**Utilisation** :
```bash
cd /path/to/Ecocity
chmod +x Scripts/create-admin-enhanced.sh
./Scripts/create-admin-enhanced.sh
```

## Initialisation Automatique

Le fichier `Data/ApplicationDbInitializer.cs` crée automatiquement les admins par défaut lors du premier démarrage :

```csharp
// Création de l'utilisateur administrateur par défaut
string adminEmail = "admin@ecocity.com";
string adminPassword = "Admin@123";

// Création de l'utilisateur
var adminUser = new ApplicationUser { ... };
await userManager.CreateAsync(adminUser, adminPassword);

// Ajout des rôles
await userManager.AddToRoleAsync(adminUser, "Admin");
await adminService.CreateAdminAsync(adminUser.Id, "Admin", "System");
```

## Sécurité

### Recommandations de Production

1. **Changer les mots de passe par défaut**
   ```bash
   # Via l'endpoint API
   curl -X POST https://votre-domaine.com/Admin/Account/RegisterAdmin \
     -H "Content-Type: application/json" \
     -d '{
       "SecretKey": "VOTRE_SECRET_KEY",
       "Email": "admin@ecocity.com",
       "Password": "NouveauMotDePasseSecure123!"
     }'
   ```

2. **Changer la clé secrète** dans `appsettings.json`
   ```json
   {
     "AdminSettings": {
       "SecretKey": "Générer-une-clé-unique-et-longue"
     }
   }
   ```

3. **Utiliser HTTPS** en production

4. **Limiter l'accès** à l'endpoint `/Admin/Account/RegisterAdmin` via un firewall ou VPN

5. **Activer l'authentification à deux facteurs** (2FA) pour les admins

## Structure des Données

### Table Admins

```sql
CREATE TABLE Admins (
    Id INT PRIMARY KEY,
    UserId NVARCHAR(450) FOREIGN KEY REFERENCES AspNetUsers(Id),
    Role NVARCHAR(50),
    Department NVARCHAR(100),
    Permissions NVARCHAR(MAX),
    IsActive BIT,
    CreatedAt DATETIME2,
    LastLoginAt DATETIME2,
    CreatedBy NVARCHAR(450),
    Notes NVARCHAR(MAX),
    InitiativesApproved INT,
    InitiativesRejected INT,
    ActionsCount INT,
    LastActionAt DATETIME2
)
```

### Rôles Identity

- `Admin` : Accès complet au panneau d'administration
- `Moderator` : Accès limité (modération des initiatives)
- `User` : Utilisateur standard (pas d'accès admin)

## Dépannage

### Erreur "Accès refusé"

**Cause** : L'utilisateur n'est pas dans la table `Admins`

**Solution** : 
1. Vérifier que l'utilisateur est admin : `SELECT * FROM Admins WHERE UserId = '...'`
2. Ajouter l'utilisateur via l'endpoint API ou script

### Erreur "Clé secrète invalide"

**Cause** : La clé secrète ne correspond pas à celle dans `appsettings.json`

**Solution** : Vérifier et synchroniser la clé secrète

### Login admin ne fonctionne pas

**Causes possibles** :
1. L'utilisateur n'est pas admin
2. Le mot de passe est incorrect
3. L'utilisateur est verrouillé

**Solutions** :
1. Vérifier le statut admin via SQL
2. Réinitialiser le mot de passe via UserManager
3. Déverrouiller le compte : `await userManager.SetLockoutEnabledAsync(user, false);`

## Exemples d'Utilisation

### Créer un admin via curl

```bash
curl -X POST http://localhost:5000/Admin/Account/RegisterAdmin \
  -H "Content-Type: application/json" \
  -d '{
    "SecretKey": "EcoCity-Admin-Secret-Key-2024-Change-In-Production",
    "Email": "admin2@ecocity.com",
    "Password": "Admin@123",
    "UserName": "admin2",
    "FirstName": "Admin",
    "LastName": "Deux",
    "Role": "Admin"
  }'
```

### Promouvoir un utilisateur existant en admin

```bash
curl -X POST http://localhost:5000/Admin/Account/RegisterAdmin \
  -H "Content-Type: application/json" \
  -d '{
    "SecretKey": "EcoCity-Admin-Secret-Key-2024-Change-In-Production",
    "Email": "existinguser@ecocity.com",
    "Role": "Admin"
  }'
```

### Vérifier si un utilisateur est admin

```bash
curl -X GET http://localhost:5000/Admin/Account/CheckAdminStatus \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Support

Pour toute question ou problème, contactez l'équipe technique EcoCity.
