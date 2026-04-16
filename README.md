# EcoCity

Une application web construite avec ASP.NET Core MVC pour la gestion d'initiatives écologiques et l'engagement communautaire.

## Fonctionnalités

- **Authentification et Autorisation** - Contrôle d'accès basé sur les rôles (Admin, Modérateur, Utilisateur)
- **Gestion des Initiatives** - Créer, gérer et suivre les initiatives écologiques
- **Système de Vote** - Vote communautaire sur les initiatives
- **Commentaires** - Discussion et retour d'information sur les initiatives
- **Notifications** - Notifications en temps réel pour les utilisateurs
- **Tableau de Bord Admin** - Outils d'administration pour gérer les utilisateurs et le contenu
- **Journalisation** - Journalisation complète avec Serilog

## Stack Technique

- **Framework** : ASP.NET Core MVC
- **Base de données** : MySQL avec Entity Framework Core
- **Authentification** : ASP.NET Identity
- **Journalisation** : Serilog
- **Documentation API** : Swagger

## Structure du Projet

```
├── Areas/          - Modules fonctionnels (Admin, etc.)
├── Controllers/    - Contrôleurs MVC
├── Data/          - Contexte de base de données et initialisation
├── Models/        - Modèles de domaine
├── Services/      - Services de logique métier
├── ViewModels/    - Modèles de vue
├── Views/         - Vues Razor
├── wwwroot/       - Fichiers statiques
└── Scripts/       - Scripts de base de données et utilitaires
```

## Démarrage

1. Configurez votre chaîne de connexion MySQL dans `appsettings.json`
2. Exécutez les migrations de base de données :
   ```bash
   dotnet ef database update
   ```
3. Lancez l'application :
   ```bash
   dotnet run
   ```

## Identifiants Admin par Défaut

L'application initialise un utilisateur administrateur lors du premier démarrage. Consultez l'initialisation de la base de données pour les identifiants.

## Documentation API

En mode développement, l'interface Swagger UI est disponible à l'adresse `/swagger`.
