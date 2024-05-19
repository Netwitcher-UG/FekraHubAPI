using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_TeacherAttendances_TeacherAttendanceId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceStatuses_StudentAttendances_StudentAttendanceId",
                table: "AttendanceStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_StudentAttendances_StudentAttendanceId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_TeacherAttendances_TeacherAttendanceId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_StudentAttendances_StudentAttendanceId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "AttendanceStatusTeacherAttendance");

            migrationBuilder.DropIndex(
                name: "IX_Students_StudentAttendanceId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Courses_StudentAttendanceId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TeacherAttendanceId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceStatuses_StudentAttendanceId",
                table: "AttendanceStatuses");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TeacherAttendanceId",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "ad674321-f625-4b73-89c1-e4c2e3e2d9c9" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ad674321-f625-4b73-89c1-e4c2e3e2d9c9");

            migrationBuilder.DropColumn(
                name: "StudentAttendanceId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "StudentAttendanceId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "TeacherAttendanceId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StudentAttendanceId",
                table: "AttendanceStatuses");

            migrationBuilder.DropColumn(
                name: "TeacherAttendanceId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "CourseID",
                table: "TeacherAttendances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusID",
                table: "TeacherAttendances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherID",
                table: "TeacherAttendances",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseID",
                table: "StudentAttendances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusID",
                table: "StudentAttendances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentID",
                table: "StudentAttendances",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ActiveUser", "Birthday", "Birthplace", "City", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "EmergencyPhoneNumber", "FirstName", "Gender", "Graduation", "ImageUser", "Job", "LastName", "LockoutEnabled", "LockoutEnd", "Name", "Nationality", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Street", "StreetNr", "TwoFactorEnabled", "UserName", "ZipCode" },
                values: new object[] { "8408394f-8eab-4825-bfac-fab6fc9b145a", 0, null, new DateTime(1985, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "city", "New York", "df11ac27-4372-4ee2-a3eb-44aaec05305d", "ApplicationUser", "admin@admin.com", false, null, "John", "Male", "Bachelor of Science in Computer Science", null, "Software Developer", "Doe", false, null, "admin", "American", "admin@admin.com", "ADMIN", "AQAAAAIAAYagAAAAEHrsblilNdIyg/FnWy+oRsZiJ92DHYvVNv5mEHyGbpeYI8iCBK7zipjaP9Zg/Dg22Q==", "+1234567890", false, "93b608b0-4bb6-4b75-826f-0f57481b1c6a", "123 Main St", "1A", false, "Admin", "10001" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "8408394f-8eab-4825-bfac-fab6fc9b145a" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_CourseID",
                table: "TeacherAttendances",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_StatusID",
                table: "TeacherAttendances",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_TeacherID",
                table: "TeacherAttendances",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_CourseID",
                table: "StudentAttendances",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_StatusID",
                table: "StudentAttendances",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_StudentID",
                table: "StudentAttendances",
                column: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAttendances_AttendanceStatuses_StatusID",
                table: "StudentAttendances",
                column: "StatusID",
                principalTable: "AttendanceStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAttendances_Courses_CourseID",
                table: "StudentAttendances",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAttendances_Students_StudentID",
                table: "StudentAttendances",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAttendances_AspNetUsers_TeacherID",
                table: "TeacherAttendances",
                column: "TeacherID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAttendances_AttendanceStatuses_StatusID",
                table: "TeacherAttendances",
                column: "StatusID",
                principalTable: "AttendanceStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAttendances_Courses_CourseID",
                table: "TeacherAttendances",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentAttendances_AttendanceStatuses_StatusID",
                table: "StudentAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAttendances_Courses_CourseID",
                table: "StudentAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAttendances_Students_StudentID",
                table: "StudentAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAttendances_AspNetUsers_TeacherID",
                table: "TeacherAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAttendances_AttendanceStatuses_StatusID",
                table: "TeacherAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAttendances_Courses_CourseID",
                table: "TeacherAttendances");

            migrationBuilder.DropIndex(
                name: "IX_TeacherAttendances_CourseID",
                table: "TeacherAttendances");

            migrationBuilder.DropIndex(
                name: "IX_TeacherAttendances_StatusID",
                table: "TeacherAttendances");

            migrationBuilder.DropIndex(
                name: "IX_TeacherAttendances_TeacherID",
                table: "TeacherAttendances");

            migrationBuilder.DropIndex(
                name: "IX_StudentAttendances_CourseID",
                table: "StudentAttendances");

            migrationBuilder.DropIndex(
                name: "IX_StudentAttendances_StatusID",
                table: "StudentAttendances");

            migrationBuilder.DropIndex(
                name: "IX_StudentAttendances_StudentID",
                table: "StudentAttendances");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "8408394f-8eab-4825-bfac-fab6fc9b145a" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8408394f-8eab-4825-bfac-fab6fc9b145a");

            migrationBuilder.DropColumn(
                name: "CourseID",
                table: "TeacherAttendances");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "TeacherAttendances");

            migrationBuilder.DropColumn(
                name: "TeacherID",
                table: "TeacherAttendances");

            migrationBuilder.DropColumn(
                name: "CourseID",
                table: "StudentAttendances");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "StudentAttendances");

            migrationBuilder.DropColumn(
                name: "StudentID",
                table: "StudentAttendances");

            migrationBuilder.AddColumn<int>(
                name: "StudentAttendanceId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Location",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StudentAttendanceId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherAttendanceId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentAttendanceId",
                table: "AttendanceStatuses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherAttendanceId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceStatusTeacherAttendance",
                columns: table => new
                {
                    AttendanceStatusId = table.Column<int>(type: "int", nullable: false),
                    TeacherAttendanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceStatusTeacherAttendance", x => new { x.AttendanceStatusId, x.TeacherAttendanceId });
                    table.ForeignKey(
                        name: "FK_AttendanceStatusTeacherAttendance_AttendanceStatuses_AttendanceStatusId",
                        column: x => x.AttendanceStatusId,
                        principalTable: "AttendanceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceStatusTeacherAttendance_TeacherAttendances_TeacherAttendanceId",
                        column: x => x.TeacherAttendanceId,
                        principalTable: "TeacherAttendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ActiveUser", "Birthday", "Birthplace", "City", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "EmergencyPhoneNumber", "FirstName", "Gender", "Graduation", "ImageUser", "Job", "LastName", "LockoutEnabled", "LockoutEnd", "Name", "Nationality", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Street", "StreetNr", "TeacherAttendanceId", "TwoFactorEnabled", "UserName", "ZipCode" },
                values: new object[] { "ad674321-f625-4b73-89c1-e4c2e3e2d9c9", 0, null, new DateTime(1985, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "city", "New York", "955b9587-4d74-4e29-b37d-f5f31b358f82", "ApplicationUser", "admin@admin.com", false, null, "John", "Male", "Bachelor of Science in Computer Science", null, "Software Developer", "Doe", false, null, "admin", "American", "admin@admin.com", "ADMIN", "AQAAAAIAAYagAAAAEAPNfzlmk1yojtWFZ10o/xycbSQRqGDD9SYy+eAn0C34kxV62PgAJ6mpNVMWPPmbmg==", "+1234567890", false, "2218167c-ecd8-42f3-8c00-f53993de5a0c", "123 Main St", "1A", null, false, "Admin", "10001" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "ad674321-f625-4b73-89c1-e4c2e3e2d9c9" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentAttendanceId",
                table: "Students",
                column: "StudentAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_StudentAttendanceId",
                table: "Courses",
                column: "StudentAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeacherAttendanceId",
                table: "Courses",
                column: "TeacherAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceStatuses_StudentAttendanceId",
                table: "AttendanceStatuses",
                column: "StudentAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TeacherAttendanceId",
                table: "AspNetUsers",
                column: "TeacherAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceStatusTeacherAttendance_TeacherAttendanceId",
                table: "AttendanceStatusTeacherAttendance",
                column: "TeacherAttendanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_TeacherAttendances_TeacherAttendanceId",
                table: "AspNetUsers",
                column: "TeacherAttendanceId",
                principalTable: "TeacherAttendances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceStatuses_StudentAttendances_StudentAttendanceId",
                table: "AttendanceStatuses",
                column: "StudentAttendanceId",
                principalTable: "StudentAttendances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_StudentAttendances_StudentAttendanceId",
                table: "Courses",
                column: "StudentAttendanceId",
                principalTable: "StudentAttendances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_TeacherAttendances_TeacherAttendanceId",
                table: "Courses",
                column: "TeacherAttendanceId",
                principalTable: "TeacherAttendances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_StudentAttendances_StudentAttendanceId",
                table: "Students",
                column: "StudentAttendanceId",
                principalTable: "StudentAttendances",
                principalColumn: "Id");
        }
    }
}
