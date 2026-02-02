using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcoCity.Models;

namespace EcoCity.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Initiative> Initiatives { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuration des noms de tables Identity
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            
            // Configuration de la table Logs
            modelBuilder.Entity<Log>(entity =>
            {
                entity.Property(e => e.Level).HasMaxLength(50);
                entity.Property(e => e.Source).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Configuration des relations et contraintes
            modelBuilder.Entity<Initiative>()
                .HasOne(i => i.User)
                .WithMany(u => u.Initiatives)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Initiative)
                .WithMany(i => i.Comments)
                .HasForeignKey(c => c.InitiativeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Initiative)
                .WithMany(i => i.Votes)
                .HasForeignKey(v => v.InitiativeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Création d'un index unique pour éviter les votes multiples
            modelBuilder.Entity<Vote>()
                .HasIndex(v => v.UserInitiativeKey)
                .IsUnique();

            // Ajout de données initiales pour les catégories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Environnement", Description = "Initiatives liées à la protection de l'environnement" },
                new Category { Id = 2, Name = "Transport", Description = "Initiatives liées aux transports et à la mobilité" },
                new Category { Id = 3, Name = "Sécurité", Description = "Initiatives liées à la sécurité des citoyens" },
                new Category { Id = 4, Name = "Culture", Description = "Initiatives culturelles et artistiques" },
                new Category { Id = 5, Name = "Social", Description = "Initiatives à caractère social et solidaire" }
            );
        }
    }
}
