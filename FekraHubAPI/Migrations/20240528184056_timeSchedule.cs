using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class timeSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartDate",
                table: "CourseSchedules",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndDate",
                table: "CourseSchedules",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

       
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
       

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "CourseSchedules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "CourseSchedules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

        }
    }
}
