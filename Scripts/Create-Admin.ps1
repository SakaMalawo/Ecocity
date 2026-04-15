# Script de création d'administrateur pour EcoCity
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
    Write-ColorOutput "    ECOCITY - Création d'Administrateur" "Cyan"
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
    
    return @{
        Email = $email
        UserName = $username
        FirstName = $firstName
        LastName = $lastName
        Password = $passwordPlain
        Role = $role
    }
}

function Create-AdminUser {
    param($UserInfo)
    
    Write-ColorOutput "Création de l'utilisateur administrateur..." "Yellow"
    
    try {
        # Créer un script SQL temporaire
        $sqlScript = @"
-- Création d'un utilisateur administrateur pour EcoCity
-- Généré le: $(Get-Date)

-- Vérifier si l'utilisateur existe déjà
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = '$($UserInfo.UserName)' OR Email = '$($UserInfo.Email)')
BEGIN
    -- Créer l'utilisateur
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                            PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
                            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName,
                            CreatedAt, Location, Bio, IsActive)
    VALUES (
        NEWID(),
        '$($UserInfo.UserName)',
        UPPER('$($UserInfo.UserName)'),
        '$($UserInfo.Email)',
        UPPER('$($UserInfo.Email)'),
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
        '$($UserInfo.FirstName)',
        '$($UserInfo.LastName)',
        GETDATE(),
        NULL,
        NULL,
        1
    );
    
    -- Ajouter le rôle
    DECLARE @UserId UNIQUEIDENTIFIER;
    SELECT @UserId = Id FROM AspNetUsers WHERE UserName = '$($UserInfo.UserName)';
    
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT @UserId, Id FROM AspNetRoles WHERE NormalizedName = UPPER('$($UserInfo.Role)');
    
    PRINT 'Utilisateur administrateur créé avec succès!';
END
ELSE
BEGIN
    PRINT 'L utilisateur existe déjà!';
END
"@
        
        # Sauvegarder le script SQL
        $sqlFile = "temp-admin-creation.sql"
        $sqlScript | Out-File -FilePath $sqlFile -Encoding UTF8
        
        Write-ColorOutput "Script SQL généré: $sqlFile" "Green"
        Write-Host ""
        Write-ColorOutput "Pour terminer la création de l'administrateur:" "Yellow"
        Write-Host "1. Appliquez les migrations avec: dotnet ef database update"
        Write-Host "2. Exécutez le script SQL: $sqlFile"
        Write-Host "3. Démarrez l'application et utilisez la méthode CreateAdmin dans le Program.cs"
        Write-Host ""
        Write-ColorOutput "Ou utilisez la méthode directe ci-dessous:" "Cyan"
        
        # Créer un code C# pour la création directe
        $csharpCode = @"
// Ajoutez ce code dans votre Program.cs pour créer l'administrateur directement:

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Créer le rôle s'il n'existe pas
    if (!await roleManager.RoleExistsAsync("$($UserInfo.Role)"))
    {
        await roleManager.CreateAsync(new IdentityRole("$($UserInfo.Role)"));
    }
    
    // Créer l'utilisateur
    var user = new ApplicationUser
    {
        UserName = "$($UserInfo.UserName)",
        Email = "$($UserInfo.Email)",
        FirstName = "$($UserInfo.FirstName)",
        LastName = "$($UserInfo.LastName)",
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    
    var result = await userManager.CreateAsync(user, "$($UserInfo.Password)");
    
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, "$($UserInfo.Role)");
        Console.WriteLine("Administrateur créé avec succès!");
    }
    else
    {
        Console.WriteLine("Erreur lors de la création: " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
"@
        
        $csharpFile = "CreateAdmin-Code.cs"
        $csharpCode | Out-File -FilePath $csharpFile -Encoding UTF8
        
        Write-ColorOutput "Code C# généré: $csharpFile" "Green"
        
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
    Write-ColorOutput "    ADMINISTRATEUR CRÉÉ AVEC SUCCÈS!" "Green"
    Write-ColorOutput "========================================" "Green"
    Write-Host ""
    Write-ColorOutput "Informations de l'administrateur:" "Cyan"
    Write-Host "  Email: $($UserInfo.Email)"
    Write-Host "  Nom d'utilisateur: $($UserInfo.UserName)"
    Write-Host "  Nom complet: $($UserInfo.FirstName) $($UserInfo.LastName)"
    Write-Host "  Rôle: $($UserInfo.Role)"
    Write-Host ""
    Write-ColorOutput "URL de connexion: https://localhost:5001/admin/login" "Yellow"
    Write-Host ""
    Write-ColorOutput "N'oubliez pas de:" "Yellow"
    Write-Host "1. Appliquer les migrations: dotnet ef database update"
    Write-Host "2. Ajouter le code de création dans Program.cs"
    Write-Host "3. Démarrer l'application"
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
