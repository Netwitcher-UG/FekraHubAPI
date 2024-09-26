using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class ExternalEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageSenderExternalEmail",
                columns: table => new
                {
                    MessageSenderId = table.Column<int>(type: "int", nullable: false),
                    ExternalEmailId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageSenderExternalEmail", x => new { x.MessageSenderId, x.ExternalEmailId });
                    table.ForeignKey(
                        name: "FK_MessageSenderExternalEmail_ExternalEmails_ExternalEmailId",
                        column: x => x.ExternalEmailId,
                        principalTable: "ExternalEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageSenderExternalEmail_MessageSenders_MessageSenderId",
                        column: x => x.MessageSenderId,
                        principalTable: "MessageSenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageSenderExternalEmail_ExternalEmailId",
                table: "MessageSenderExternalEmail",
                column: "ExternalEmailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageSenderExternalEmail");

            migrationBuilder.DropTable(
                name: "ExternalEmails");
        }
    }
}
