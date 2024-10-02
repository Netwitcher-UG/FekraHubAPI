using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class messageSenderSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "note",
                table: "ExternalEmails");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "MessageSenders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject",
                table: "MessageSenders");

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "ExternalEmails",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
