#!/bin/bash

# Script de création d'administrateur pour EcoCity
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
    echo -e "${CYAN}    ECOCITY - Création d'Administrateur${NC}"
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
}

function create_admin_user() {
    log_warning "Création de l'utilisateur administrateur..."
    
    # Créer un script SQL temporaire
    cat > temp-admin-creation.sql << EOF
-- Création d'un utilisateur administrateur pour EcoCity
-- Généré le: $(date)

-- Vérifier si l'utilisateur existe déjà
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = '$username' OR Email = '$email')
BEGIN
    -- Créer l'utilisateur
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                            PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
                            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName,
                            CreatedAt, Location, Bio, IsActive)
    VALUES (
        NEWID(),
        '$username',
        UPPER('$username'),
        '$email',
        UPPER('$email'),
        1,
        -- Le hash du mot de passe sera généré par l'application
        '',
        NEWID(),
        NEWID(),
        NULL,
        0,
        0,
        NULL,
        1,
        0,
        '$firstName',
        '$lastName',
        GETDATE(),
        NULL,
        NULL,
        1
    );
    
    -- Ajouter le rôle
    DECLARE @UserId UNIQUEIDENTIFIER;
    SELECT @UserId = Id FROM AspNetUsers WHERE UserName = '$username';
    
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT @UserId, Id FROM AspNetRoles WHERE NormalizedName = UPPER('$role');
    
    PRINT 'Utilisateur administrateur créé avec succès!';
END
ELSE
BEGIN
    PRINT 'L utilisateur existe déjà!';
END
EOF
    
    log_success "Script SQL généré: temp-admin-creation.sql"
    echo ""
    log_warning "Pour terminer la création de l'administrateur:"
    echo "1. Appliquez les migrations avec: dotnet ef database update"
    echo "2. Exécutez le script SQL: temp-admin-creation.sql"
    echo "3. Démarrez l'application et utilisez la méthode CreateAdmin dans le Program.cs"
    echo ""
    log_info "Ou utilisez la méthode directe ci-dessous:"
    
    # Créer un code C# pour la création directe
    cat > CreateAdmin-Code.cs << EOF
// Ajoutez ce code dans votre Program.cs pour créer l'administrateur directement:

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Créer le rôle s'il n'existe pas
    if (!await roleManager.RoleExistsAsync("$role"))
    {
        await roleManager.CreateAsync(new IdentityRole("$role"));
    }
    
    // Créer l'utilisateur
    var user = new ApplicationUser
    {
        UserName = "$username",
        Email = "$email",
        FirstName = "$firstName",
        LastName = "$lastName",
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    
    var result = await userManager.CreateAsync(user, "$password");
    
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, "$role");
        Console.WriteLine("Administrateur créé avec succès!");
    }
    else
    {
        Console.WriteLine("Erreur lors de la création: " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
EOF
    
    log_success "Code C# généré: CreateAdmin-Code.cs"
}

function show_success() {
    echo ""
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}    ADMINISTRATEUR CRÉÉ AVEC SUCCÈS!${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""
    log_info "Informations de l'administrateur:"
    echo "  Email: $email"
    echo "  Nom d'utilisateur: $username"
    echo "  Nom complet: $firstName $lastName"
    echo "  Rôle: $role"
    echo ""
    log_warning "URL de connexion: https://localhost:5001/admin/login"
    echo ""
    log_warning "N'oubliez pas de:"
    echo "1. Appliquer les migrations: dotnet ef database update"
    echo "2. Ajouter le code de création dans Program.cs"
    echo "3. Démarrer l'application"
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
