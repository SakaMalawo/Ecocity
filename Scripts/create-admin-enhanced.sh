#!/bin/bash

# Script de création d'administrateur pour EcoCity (Version Améliorée)
# Ce script crée un administrateur dans Identity ET dans la table Admin
# Auteur: EcoCity Team
# Date: $(date +%Y-%m-%d)

# Couleurs pour la console
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

function show_banner() {
    clear
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}  ECOCITY - Création d'Administrateur${NC}"
    echo -e "${CYAN}       (Version Améliorée)${NC}"
    echo -e "${CYAN}========================================${NC}"
    echo ""
}

function log_info() {
    echo -e "${WHITE}$1${NC}"
}

function log_success() {
    echo -e "${GREEN}$1${NC}"
}

function log_warning() {
    echo -e "${YELLOW}$1${NC}"
}

function log_error() {
    echo -e "${RED}$1${NC}"
}

function test_prerequisites() {
    log_warning "Vérification des prérequis..."
    
    # Vérifier si nous sommes dans le bon répertoire
    if [ ! -f "EcoCity.csproj" ]; then
        log_error "Erreur: Veuillez exécuter ce script depuis le répertoire racine du projet EcoCity."
        exit 1
    fi
    
    # Vérifier si dotnet est installé
    if ! command -v dotnet &> /dev/null; then
        log_error "Erreur: .NET CLI n'est pas installé."
        exit 1
    fi
    
    local dotnetVersion=$(dotnet --version)
    log_success "Version .NET: $dotnetVersion"
    
    log_success "Prérequis vérifiés avec succès!"
    echo ""
}

function get_user_input() {
    echo -e "${CYAN}=== INFORMATIONS DE L'ADMINISTRATEUR ===${NC}"
    echo ""
    
    # Email
    while true; do
        read -p "Email de l'administrateur: " email
        if [[ "$email" =~ ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$ ]]; then
            break
        else
            log_error "Veuillez entrer une adresse email valide."
        fi
    done
    
    # Nom d'utilisateur
    while true; do
        read -p "Nom d'utilisateur: " username
        if [ ${#username} -ge 3 ]; then
            break
        else
            log_error "Le nom d'utilisateur doit contenir au moins 3 caractères."
        fi
    done
    
    # Prénom
    while true; do
        read -p "Prénom: " firstName
        if [ -n "$firstName" ]; then
            break
        else
            log_error "Le prénom ne peut pas être vide."
        fi
    done
    
    # Nom
    while true; do
        read -p "Nom de famille: " lastName
        if [ -n "$lastName" ]; then
            break
        else
            log_error "Le nom de famille ne peut pas être vide."
        fi
    done
    
    # Mot de passe
    while true; do
        read -s -p "Mot de passe (minimum 8 caractères): " password
        echo ""
        if [ ${#password} -ge 8 ]; then
            break
        else
            log_error "Le mot de passe doit contenir au moins 8 caractères."
        fi
    done
    
    # Confirmation du mot de passe
    while true; do
        read -s -p "Confirmez le mot de passe: " confirmPassword
        echo ""
        if [ "$password" = "$confirmPassword" ]; then
            break
        else
            log_error "Les mots de passe ne correspondent pas."
        fi
    done
    
    # Rôle
    echo ""
    log_warning "Rôle à attribuer:"
    echo "1. Admin (accès complet)"
    echo "2. Moderator (accès limité)"
    
    while true; do
        read -p "Choix (1 ou 2): " roleChoice
        case $roleChoice in
            1) role="Admin"; break ;;
            2) role="Moderator"; break ;;
            *) log_error "Choix invalide. Veuillez sélectionner 1 ou 2." ;;
        esac
    done
    
    # Département (optionnel)
    read -p "Département (optionnel): " department
    
    # Notes (optionnel)
    read -p "Notes internes (optionnel): " notes
}

function create_admin_user() {
    log_warning "Création de l'utilisateur administrateur..."
    
    # Créer le code C# pour la création directe
    cat > CreateAdmin-Enhanced-Code.cs << EOF
// Ajoutez ce code dans votre Program.cs pour créer l'administrateur directement:
// Placez ce code avant 'app.Run()'

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var adminService = services.GetRequiredService<IAdminService>();
    
    // Créer le rôle s'il n'existe pas
    if (!await roleManager.RoleExistsAsync("$role"))
    {
        await roleManager.CreateAsync(new IdentityRole("$role"));
    }
    
    // Vérifier si l'utilisateur existe déjà
    var existingUser = await userManager.FindByEmailAsync("$email");
    if (existingUser == null)
    {
        // Créer l'utilisateur
        var user = new ApplicationUser
        {
            UserName = "$username",
            Email = "$email",
            FirstName = "$firstName",
            LastName = "$lastName",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        var result = await userManager.CreateAsync(user, "$password");
        
        if (result.Succeeded)
        {
            // Ajouter le rôle Identity
            await userManager.AddToRoleAsync(user, "$role");
            
            // Créer l'entrée dans la table Admin
            await adminService.CreateAdminAsync(user.Id, "$role", "System");
            
            Console.WriteLine("Administrateur créé avec succès!");
            Console.WriteLine($"Email: $email");
            Console.WriteLine($"Rôle: $role");
            Console.WriteLine($"Nom: $firstName $lastName");
        }
        else
        {
            Console.WriteLine("Erreur lors de la création de l'utilisateur:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
        }
    }
    else
    {
        // L'utilisateur existe, vérifier s'il est déjà admin
        var isAdmin = await adminService.IsUserAdminAsync(existingUser.Id);
        if (!isAdmin)
        {
            await userManager.AddToRoleAsync(existingUser, "$role");
            await adminService.CreateAdminAsync(existingUser.Id, "$role", "System");
            Console.WriteLine("Utilisateur existant promu administrateur!");
        }
        else
        {
            Console.WriteLine("L'utilisateur est déjà administrateur!");
        }
    }
}
EOF
    
    # Créer un script SQL pour la table Admin
    cat > CreateAdmin-Enhanced-SQL.sql << EOF
-- Script SQL pour créer manuellement l'administrateur dans la table Admin
-- Exécutez ce script après avoir créé l'utilisateur avec le code C# ci-dessus

-- Vérifier si l'admin existe déjà
IF NOT EXISTS (SELECT 1 FROM Admins WHERE UserId = (SELECT Id FROM AspNetUsers WHERE UserName = '$username'))
BEGIN
    -- Créer l'entrée dans la table Admin
    INSERT INTO Admins (UserId, Role, Department, Permissions, IsActive, CreatedAt, CreatedBy, Notes)
    SELECT 
        Id,
        '$role',
        '$department',
        '{}',
        1,
        GETDATE(),
        'System',
        '$notes'
    FROM AspNetUsers 
    WHERE UserName = '$username';
    
    PRINT 'Administrateur créé dans la table Admin!';
END
ELSE
BEGIN
    PRINT 'L administrateur existe déjà dans la table Admin!';
END
EOF
    
    log_success "Fichiers générés:"
    log_info "- Code C#: CreateAdmin-Enhanced-Code.cs"
    log_info "- Script SQL: CreateAdmin-Enhanced-SQL.sql"
}

function show_success() {
    echo ""
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}  ADMINISTRATEUR CRÉÉ AVEC SUCCÈS!${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""
    log_info "Informations de l'administrateur:"
    echo "  Email: $email"
    echo "  Nom d'utilisateur: $username"
    echo "  Nom complet: $firstName $lastName"
    echo "  Rôle: $role"
    if [ -n "$department" ]; then
        echo "  Département: $department"
    fi
    echo ""
    log_warning "URL de connexion: https://localhost:5001/admin/login"
    echo ""
    log_warning "Prochaines étapes:"
    echo "1. Appliquer les migrations: dotnet ef database update"
    echo "2. Ajouter le code de création dans Program.cs"
    echo "3. Démarrer l'application"
    echo "4. Se connecter avec les identifiants créés"
    echo ""
    log_success "Fonctionnalités disponibles:"
    echo "- Dashboard admin avec statistiques en temps réel"
    echo "- Approbation/rejet d'initiatives avec notifications"
    echo "- Suivi des statistiques de chaque administrateur"
    echo "- Gestion des utilisateurs et permissions"
    echo ""
}

# Programme principal
set -e

try {
    show_banner
    test_prerequisites
    get_user_input
    
    echo ""
    log_warning "Résumé des informations:"
    echo "Email: $email"
    echo "Nom d'utilisateur: $username"
    echo "Nom: $firstName $lastName"
    echo "Rôle: $role"
    if [ -n "$department" ]; then
        echo "Département: $department"
    fi
    echo ""
    
    read -p "Confirmer la création? (O/N) " confirm
    if [[ "$confirm" =~ ^[Oo]$ ]]; then
        create_admin_user
        show_success
    else
        log_warning "Création annulée."
    fi
}
catch {
    log_error "Erreur inattendue: $1"
    exit 1
}

echo ""
log_info "Appuyez sur Entrée pour quitter..."
read
