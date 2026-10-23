using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReportDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_StudentId",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "ReportMonth",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReportYear",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            // Update old reports using CreationDate
            migrationBuilder.Sql(@"
                UPDATE ""Reports""
                SET
                    ""ReportMonth"" = EXTRACT(
                        MONTH FROM ""CreationDate"" AT TIME ZONE 'UTC'
                    )::int,
                    ""ReportYear"" = EXTRACT(
                        YEAR FROM ""CreationDate"" AT TIME ZONE 'UTC'
                    )::int;
            ");
            migrationBuilder.CreateIndex(
                name: "UX_Reports_Student_Period",
                table: "Reports",
                columns: new[] { "StudentId", "ReportYear", "ReportMonth" },
                unique: true);
        }

        /// <inheritdoc />   
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Reports_Student_Period",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportMonth",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportYear",
                table: "Reports");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StudentId",
                table: "Reports",
                column: "StudentId");
        }
    }
}
