using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCity.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO Categories (Name, Description) VALUES ('Environnement', 'Initiatives pour la protection de lenvironnement et la durabilité')");
            migrationBuilder.Sql("INSERT INTO Categories (Name, Description) VALUES ('Éducation', 'Projets éducatifs et de sensibilisation')");
            migrationBuilder.Sql("INSERT INTO Categories (Name, Description) VALUES ('Social', 'Actions sociales et communautaires')");
            migrationBuilder.Sql("INSERT INTO Categories (Name, Description) VALUES ('Culture', 'Initiatives culturelles et artistiques')");
            migrationBuilder.Sql("INSERT INTO Categories (Name, Description) VALUES ('Santé', 'Projets liés à la santé et au bien-être')");
            migrationBuilder.Sql("INSERT INTO Categories (Name, Description) VALUES ('Technologie', 'Innovations technologiques et numériques')");
            migrationBuilder.Sql("INSERT INTO Categories (Name, Description) VALUES ('Urbanisme', 'Aménagement urbain et projets de ville')");
            migrationBuilder.Sql("INSERT INTO Categories (Name, Description) VALUES ('Énergie', 'Projets liés aux énergies renouvelables et à lefficacité énergétique')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Categories WHERE Name IN ('Environnement', 'Éducation', 'Social', 'Culture', 'Santé', 'Technologie', 'Urbanisme', 'Énergie')");
        }
    }
}
