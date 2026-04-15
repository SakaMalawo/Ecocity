# Script de création d'administrateur pour EcoCity (Version Améliorée)
# Ce script crée un administrateur dans Identity ET dans la table Admin
# Auteur: EcoCity Team
# Date: $(Get-Date -Format "yyyy-MM-dd")

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigFile = "appsettings.json"
)

# Couleurs pour la console
$colors = @{
    Red = "Red"
    Green = "Green"
    Yellow = "Yellow"
    Cyan = "Cyan"
    White = "White"
}

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $colors[$Color]
}

function Show-Banner {
    Clear-Host
    Write-ColorOutput "========================================" "Cyan"
    Write-ColorOutput "  ECOCITY - Création d'Administrateur" "Cyan"
    Write-ColorOutput "       (Version Améliorée)" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
}

function Test-Prerequisites {
    Write-ColorOutput "Vérification des prérequis..." "Yellow"
    
    # Vérifier si nous sommes dans le bon répertoire
    if (-not (Test-Path "EcoCity.csproj")) {
        Write-ColorOutput "Erreur: Veuillez exécuter ce script depuis le répertoire racine du projet EcoCity." "Red"
        exit 1
    }
    
    # Vérifier si le fichier de configuration existe
    if (-not (Test-Path $ConfigFile)) {
        Write-ColorOutput "Erreur: Le fichier de configuration $ConfigFile n'existe pas." "Red"
        exit 1
    }
    
    # Vérifier si dotnet est installé
    try {
        $dotnetVersion = dotnet --version
        Write-ColorOutput "Version .NET: $dotnetVersion" "Green"
    }
    catch {
        Write-ColorOutput "Erreur: .NET CLI n'est pas installé." "Red"
        exit 1
    }
    
    Write-ColorOutput "Prérequis vérifiés avec succès!" "Green"
    Write-Host ""
}

function Get-UserInput {
    Write-ColorOutput "=== INFORMATIONS DE L'ADMINISTRATEUR ===" "Cyan"
    Write-Host ""
    
    # Email
    do {
        $email = Read-Host "Email de l'administrateur"
        if ($email -notmatch '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$') {
            Write-ColorOutput "Veuillez entrer une adresse email valide." "Red"
        }
    } while ($email -notmatch '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$')
    
    # Nom d'utilisateur
    do {
        $username = Read-Host "Nom d'utilisateur"
        if ([string]::IsNullOrWhiteSpace($username) -or $username.Length -lt 3) {
            Write-ColorOutput "Le nom d'utilisateur doit contenir au moins 3 caractères." "Red"
        }
    } while ([string]::IsNullOrWhiteSpace($username) -or $username.Length -lt 3)
    
    # Prénom
    $firstName = Read-Host "Prénom"
    while ([string]::IsNullOrWhiteSpace($firstName)) {
        Write-ColorOutput "Le prénom ne peut pas être vide." "Red"
        $firstName = Read-Host "Prénom"
    }
    
    # Nom
    $lastName = Read-Host "Nom de famille"
    while ([string]::IsNullOrWhiteSpace($lastName)) {
        Write-ColorOutput "Le nom de famille ne peut pas être vide." "Red"
        $lastName = Read-Host "Nom de famille"
    }
    
    # Mot de passe
    do {
        $password = Read-Host "Mot de passe (minimum 8 caractères)" -AsSecureString
        $passwordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
        
        if ($passwordPlain.Length -lt 8) {
            Write-ColorOutput "Le mot de passe doit contenir au moins 8 caractères." "Red"
            $passwordPlain = $null
        }
    } while ($passwordPlain.Length -lt 8)
    
    # Confirmation du mot de passe
    do {
        $confirmPassword = Read-Host "Confirmez le mot de passe" -AsSecureString
        $confirmPasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($confirmPassword))
        
        if ($passwordPlain -ne $confirmPasswordPlain) {
            Write-ColorOutput "Les mots de passe ne correspondent pas." "Red"
            $confirmPasswordPlain = $null
        }
    } while ($passwordPlain -ne $confirmPasswordPlain)
    
    # Rôle
    Write-Host ""
    Write-ColorOutput "Rôle à attribuer:" "Yellow"
    Write-Host "1. Admin (accès complet)"
    Write-Host "2. Moderator (accès limité)"
    
    do {
        $roleChoice = Read-Host "Choix (1 ou 2)"
        switch ($roleChoice) {
            "1" { $role = "Admin" }
            "2" { $role = "Moderator" }
            default { 
                Write-ColorOutput "Choix invalide. Veuillez sélectionner 1 ou 2." "Red"
                $role = $null
            }
        }
    } while ($null -eq $role)
    
    # Département (optionnel)
    $department = Read-Host "Département (optionnel)"
    
    # Notes (optionnel)
    $notes = Read-Host "Notes internes (optionnel)"
    
    return @{
        Email = $email
        UserName = $username
        FirstName = $firstName
        LastName = $lastName
        Password = $passwordPlain
        Role = $role
        Department = $department
        Notes = $notes
    }
}

function Create-AdminUser {
    param($UserInfo)
    
    Write-ColorOutput "Création de l'utilisateur administrateur..." "Yellow"
    
    try {
        # Créer le code C# pour la création directe
        $csharpCode = @"
// Ajoutez ce code dans votre Program.cs pour créer l'administrateur directement:
// Placez ce code avant 'app.Run()'

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var adminService = services.GetRequiredService<IAdminService>();
    
    // Créer le rôle s'il n'existe pas
    if (!await roleManager.RoleExistsAsync("$($UserInfo.Role)"))
    {
        await roleManager.CreateAsync(new IdentityRole("$($UserInfo.Role)"));
    }
    
    // Vérifier si l'utilisateur existe déjà
    var existingUser = await userManager.FindByEmailAsync("$($UserInfo.Email)");
    if (existingUser == null)
    {
        // Créer l'utilisateur
        var user = new ApplicationUser
        {
            UserName = "$($UserInfo.UserName)",
            Email = "$($UserInfo.Email)",
            FirstName = "$($UserInfo.FirstName)",
            LastName = "$($UserInfo.LastName)",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        var result = await userManager.CreateAsync(user, "$($UserInfo.Password)");
        
        if (result.Succeeded)
        {
            // Ajouter le rôle Identity
            await userManager.AddToRoleAsync(user, "$($UserInfo.Role)");
            
            // Créer l'entrée dans la table Admin
            await adminService.CreateAdminAsync(user.Id, "$($UserInfo.Role)", "System");
            
            Console.WriteLine("Administrateur créé avec succès!");
            Console.WriteLine($"Email: $($UserInfo.Email)");
            Console.WriteLine($"Rôle: $($UserInfo.Role)");
            Console.WriteLine($"Nom: $($UserInfo.FirstName) $($UserInfo.LastName)");
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
            await userManager.AddToRoleAsync(existingUser, "$($UserInfo.Role)");
            await adminService.CreateAdminAsync(existingUser.Id, "$($UserInfo.Role)", "System");
            Console.WriteLine("Utilisateur existant promu administrateur!");
        }
        else
        {
            Console.WriteLine("L'utilisateur est déjà administrateur!");
        }
    }
}
"@
        
        # Créer un script SQL pour la table Admin
        $sqlScript = @"
-- Script SQL pour créer manuellement l'administrateur dans la table Admin
-- Exécutez ce script après avoir créé l'utilisateur avec le code C# ci-dessus

-- Vérifier si l'admin existe déjà
IF NOT EXISTS (SELECT 1 FROM Admins WHERE UserId = (SELECT Id FROM AspNetUsers WHERE UserName = '$($UserInfo.UserName)'))
BEGIN
    -- Créer l'entrée dans la table Admin
    INSERT INTO Admins (UserId, Role, Department, Permissions, IsActive, CreatedAt, CreatedBy, Notes)
    SELECT 
        Id,
        '$($UserInfo.Role)',
        '$($UserInfo.Department)',
        '{}',
        1,
        GETDATE(),
        'System',
        '$($userInfo.Notes)'
    FROM AspNetUsers 
    WHERE UserName = '$($UserInfo.UserName)';
    
    PRINT 'Administrateur créé dans la table Admin!';
END
ELSE
BEGIN
    PRINT 'L administrateur existe déjà dans la table Admin!';
END
"@
        
        # Sauvegarder les fichiers
        $csharpFile = "CreateAdmin-Enhanced-Code.cs"
        $sqlFile = "CreateAdmin-Enhanced-SQL.sql"
        
        $csharpCode | Out-File -FilePath $csharpFile -Encoding UTF8
        $sqlScript | Out-File -FilePath $sqlFile -Encoding UTF8
        
        Write-ColorOutput "Fichiers générés:" "Green"
        Write-ColorOutput "- Code C#: $csharpFile" "Cyan"
        Write-ColorOutput "- Script SQL: $sqlFile" "Cyan"
        
        return $true
    }
    catch {
        Write-ColorOutput "Erreur lors de la création: $($_.Exception.Message)" "Red"
        return $false
    }
}

function Show-Success {
    param($UserInfo)
    
    Write-Host ""
    Write-ColorOutput "========================================" "Green"
    Write-ColorOutput "  ADMINISTRATEUR CRÉÉ AVEC SUCCÈS!" "Green"
    Write-ColorOutput "========================================" "Green"
    Write-Host ""
    Write-ColorOutput "Informations de l'administrateur:" "Cyan"
    Write-Host "  Email: $($UserInfo.Email)"
    Write-Host "  Nom d'utilisateur: $($UserInfo.UserName)"
    Write-Host "  Nom complet: $($UserInfo.FirstName) $($UserInfo.LastName)"
    Write-Host "  Rôle: $($UserInfo.Role)"
    if ($UserInfo.Department) {
        Write-Host "  Département: $($UserInfo.Department)"
    }
    Write-Host ""
    Write-ColorOutput "URL de connexion: https://localhost:5001/admin/login" "Yellow"
    Write-Host ""
    Write-ColorOutput "Prochaines étapes:" "Yellow"
    Write-Host "1. Appliquer les migrations: dotnet ef database update"
    Write-Host "2. Ajouter le code de création dans Program.cs"
    Write-Host "3. Démarrer l'application"
    Write-Host "4. Se connecter avec les identifiants créés"
    Write-Host ""
    Write-ColorOutput "Fonctionnalités disponibles:" "Green"
    Write-Host "- Dashboard admin avec statistiques en temps réel"
    Write-Host "- Approbation/rejet d'initiatives avec notifications"
    Write-Host "- Suivi des statistiques de chaque administrateur"
    Write-Host "- Gestion des utilisateurs et permissions"
    Write-Host ""
}

# Programme principal
try {
    Show-Banner
    Test-Prerequisites
    $userInfo = Get-UserInput
    
    Write-Host ""
    Write-ColorOutput "Résumé des informations:" "Yellow"
    Write-Host "Email: $($userInfo.Email)"
    Write-Host "Nom d'utilisateur: $($userInfo.UserName)"
    Write-Host "Nom: $($userInfo.FirstName) $($userInfo.LastName)"
    Write-Host "Rôle: $($userInfo.Role)"
    if ($userInfo.Department) {
        Write-Host "Département: $($userInfo.Department)"
    }
    Write-Host ""
    
    $confirm = Read-Host "Confirmer la création? (O/N)"
    if ($confirm -eq "O" -or $confirm -eq "o") {
        if (Create-AdminUser -UserInfo $userInfo) {
            Show-Success -UserInfo $userInfo
        }
    } else {
        Write-ColorOutput "Création annulée." "Yellow"
    }
}
catch {
    Write-ColorOutput "Erreur inattendue: $($_.Exception.Message)" "Red"
    exit 1
}

Write-Host ""
Write-ColorOutput "Appuyez sur Entrée pour quitter..." "White"
Read-Host
