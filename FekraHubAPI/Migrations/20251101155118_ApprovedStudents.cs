using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class ApprovedStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<bool>(
                name: "AdminApproved",
                table: "Students",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ParentApproved",
                table: "Students",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SchoolInfos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Facebook", "Instagram" },
                values: new object[] { null, null });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminApproved",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentApproved",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "SchoolInfos");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "SchoolInfos");

        }
    }
}
