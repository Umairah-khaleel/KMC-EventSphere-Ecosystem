using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMCEventSphere.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteOrganizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizers_Users_UserID",
                table: "Organizers");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizers_Users_UserID",
                table: "Organizers",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizers_Users_UserID",
                table: "Organizers");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizers_Users_UserID",
                table: "Organizers",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
