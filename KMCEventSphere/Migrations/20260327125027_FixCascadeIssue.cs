using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMCEventSphere.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Organizers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Organizers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Organizers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "Organizers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Organizers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Organizers_UserID",
                table: "Organizers",
                column: "UserID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizers_Users_UserID",
                table: "Organizers",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizers_Users_UserID",
                table: "Organizers");

            migrationBuilder.DropIndex(
                name: "IX_Organizers_UserID",
                table: "Organizers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Organizers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Organizers");

            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "Organizers");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Organizers");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Organizers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
