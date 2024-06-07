using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "7ffa1e95-ca28-4391-8496-1570a6abe88f" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7ffa1e95-ca28-4391-8496-1570a6abe88f");

            migrationBuilder.CreateTable(
                name: "AspNetPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetPermissions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetPermissions",
                columns: new[] { "Id", "Type", "Value" },
                values: new object[,]
                {
                    { 1, "GetUsers", "GetUsers" },
                    { 2, "AddUsers", "AddUsers" },
                    { 3, "GetStudentsCourse", "GetStudentsCourse" },
                    { 4, "UpdateStudentsCourse", "UpdateStudentsCourse" },
                    { 5, "GetTeachersCourse", "GetTeachersCourse" },
                    { 6, "UpdateTeachersCourse", "UpdateTeachersCourse" },
                    { 7, "GetContracts", "GetContracts" },
                    { 8, "ManageEvents", "ManageEvents" },
                    { 9, "GetEvents", "GetEvents" },
                    { 10, "GetStudentsAttendance", "GetStudentsAttendance" },
                    { 11, "UpdateStudentsAttendance", "UpdateStudentsAttendance" },
                    { 12, "GetTeachersAttendance", "GetTeachersAttendance" },
                    { 13, "UpdateTeachersAttendance", "UpdateTeachersAttendance" },
                    { 14, "InsertUpdateStudentsReports", "InsertUpdateStudentsReports" },
                    { 15, "GetStudentsReports", "GetStudentsReports" },
                    { 16, "ApproveReports", "ApproveReports" },
                    { 17, "ManagePayrolls", "ManagePayrolls" },
                    { 18, "ManageBooks", "ManageBooks" },
                    { 19, "DeleteSchoolData", "DeleteSchoolData" },
                    { 20, "ResetData", "ResetData" }
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetUsers", "GetUsers" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "AddUsers", "AddUsers" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetStudentsCourse", "GetStudentsCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateStudentsCourse", "UpdateStudentsCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "UpdateTeachersCourse", "UpdateTeachersCourse", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetContracts", "GetContracts", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ManageEvents", "ManageEvents", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetEvents", "GetEvents", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "UpdateStudentsAttendance", "UpdateStudentsAttendance", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetTeachersAttendance", "GetTeachersAttendance", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "UpdateTeachersAttendance", "UpdateTeachersAttendance", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "InsertUpdateStudentsReports", "InsertUpdateStudentsReports", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ApproveReports", "ApproveReports", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ManagePayrolls", "ManagePayrolls", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ManageBooks", "ManageBooks", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "DeleteSchoolData", "DeleteSchoolData", "1" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 5, "GetTeachersCourse", "GetTeachersCourse", "1" },
                    { 10, "GetStudentsAttendance", "GetStudentsAttendance", "1" },
                    { 15, "GetStudentsReports", "GetStudentsReports", "1" },
                    { 20, "ResetData", "ResetData", "1" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetPermissions_Value",
                table: "AspNetPermissions",
                column: "Value",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetPermissions");

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "Permissions.Manage.View", "Permissions.Manage.View" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "Permissions.Manage.Create", "Permissions.Manage.Create" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "Permissions.Manage.Edit", "Permissions.Manage.Edit" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "Permissions.Manage.Delete", "Permissions.Manage.Delete" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageS.View", "Permissions.ManageS.View", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageS.Create", "Permissions.ManageS.Create", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageS.Edit", "Permissions.ManageS.Edit", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageS.Delete", "Permissions.ManageS.Delete", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageP.View", "Permissions.ManageP.View", "3" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageP.Create", "Permissions.ManageP.Create", "3" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageP.Edit", "Permissions.ManageP.Edit", "3" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageP.Delete", "Permissions.ManageP.Delete", "3" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageT.View", "Permissions.ManageT.View", "4" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageT.Create", "Permissions.ManageT.Create", "4" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageT.Edit", "Permissions.ManageT.Edit", "4" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "Permissions.ManageT.Delete", "Permissions.ManageT.Delete", "4" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ActiveUser", "Birthday", "Birthplace", "City", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "EmergencyPhoneNumber", "FirstName", "Gender", "Graduation", "ImageUser", "Job", "LastName", "LockoutEnabled", "LockoutEnd", "Name", "Nationality", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Street", "StreetNr", "TwoFactorEnabled", "UserName", "ZipCode" },
                values: new object[] { "7ffa1e95-ca28-4391-8496-1570a6abe88f", 0, null, new DateTime(1985, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "city", "New York", "ff53fa48-da17-451e-b62d-00924d2f10ac", "ApplicationUser", "admin@admin.com", false, null, "John", "Male", "Bachelor of Science in Computer Science", null, "Software Developer", "Doe", false, null, "admin", "American", "admin@admin.com", "ADMIN", "AQAAAAIAAYagAAAAEGeBIHtMJRiswUlnTH76oEdWd74pSFxb2uc5S2W1JzuwjLkrGPd0jdjKAvGHvqj7ew==", "+1234567890", false, "bb1c794c-496a-4f51-888d-dff4bba28eba", "123 Main St", "1A", false, "Admin", "10001" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "7ffa1e95-ca28-4391-8496-1570a6abe88f" });
        }
    }
}
