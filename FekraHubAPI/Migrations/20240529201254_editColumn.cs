using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class editColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


          

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "CourseSchedules",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "CourseSchedules",
                newName: "EndTime");

            migrationBuilder.AlterColumn<byte[]>(
                name: "File",
                table: "WorkContracts",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte[]>(
                name: "File",
                table: "StudentContract",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte[]>(
                name: "File",
                table: "PayRoll",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte[]>(
                name: "File",
                table: "ParentInvoices",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

     




        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

          

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "CourseSchedules",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "CourseSchedules",
                newName: "EndDate");

            migrationBuilder.AlterColumn<byte>(
                name: "File",
                table: "WorkContracts",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<byte>(
                name: "File",
                table: "StudentContract",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<byte>(
                name: "File",
                table: "PayRoll",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<byte>(
                name: "File",
                table: "ParentInvoices",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

          

        }
    }
}
