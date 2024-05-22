using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateDB2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseUpload");

            migrationBuilder.DropTable(
                name: "ParentContracts");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "8408394f-8eab-4825-bfac-fab6fc9b145a" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8408394f-8eab-4825-bfac-fab6fc9b145a");

            migrationBuilder.AddColumn<int>(
                name: "CourseID",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UploadId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentContract",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    File = table.Column<byte>(type: "tinyint", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
               
                    StudentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentContract_Students_ParentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherCourse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CourseID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherCourse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherCourse_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherCourse_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "Permissions.Manage.View", "Permissions.Manage.View", "1" },
                    { 2, "Permissions.Manage.Create", "Permissions.Manage.Create", "1" },
                    { 3, "Permissions.Manage.Edit", "Permissions.Manage.Edit", "1" },
                    { 4, "Permissions.Manage.Delete", "Permissions.Manage.Delete", "1" },
                    { 6, "Permissions.ManageS.View", "Permissions.ManageS.View", "2" },
                    { 7, "Permissions.ManageS.Create", "Permissions.ManageS.Create", "2" },
                    { 8, "Permissions.ManageS.Edit", "Permissions.ManageS.Edit", "2" },
                    { 9, "Permissions.ManageS.Delete", "Permissions.ManageS.Delete", "2" },
                    { 11, "Permissions.ManageP.View", "Permissions.ManageP.View", "3" },
                    { 12, "Permissions.ManageP.Create", "Permissions.ManageP.Create", "3" },
                    { 13, "Permissions.ManageP.Edit", "Permissions.ManageP.Edit", "3" },
                    { 14, "Permissions.ManageP.Delete", "Permissions.ManageP.Delete", "3" },
                    { 16, "Permissions.ManageT.View", "Permissions.ManageT.View", "4" },
                    { 17, "Permissions.ManageT.Create", "Permissions.ManageT.Create", "4" },
                    { 18, "Permissions.ManageT.Edit", "Permissions.ManageT.Edit", "4" },
                    { 19, "Permissions.ManageT.Delete", "Permissions.ManageT.Delete", "4" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ActiveUser", "Birthday", "Birthplace", "City", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "EmergencyPhoneNumber", "FirstName", "Gender", "Graduation", "ImageUser", "Job", "LastName", "LockoutEnabled", "LockoutEnd", "Name", "Nationality", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Street", "StreetNr", "TwoFactorEnabled", "UserName", "ZipCode" },
                values: new object[] { "86687f5a-ab0e-4dc7-a830-367329c91e8a", 0, null, new DateTime(1985, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "city", "New York", "11f5e41f-d935-4e19-bfed-4dbc74846ede", "ApplicationUser", "admin@admin.com", false, null, "John", "Male", "Bachelor of Science in Computer Science", null, "Software Developer", "Doe", false, null, "admin", "American", "admin@admin.com", "ADMIN", "AQAAAAIAAYagAAAAEM48cY4DR49KCgE26mtOXkAJmkypPeSLnkJiLwe1U/S0GPdLYgSAWORtiSahbb+iag==", "+1234567890", false, "f0e10e67-5043-4ce0-8727-795e319f0253", "123 Main St", "1A", false, "Admin", "10001" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "86687f5a-ab0e-4dc7-a830-367329c91e8a" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_CourseID",
                table: "Students",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UploadId",
                table: "Courses",
                column: "UploadId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentContract_StudentID",
                table: "StudentContract",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCourse_CourseID",
                table: "TeacherCourse",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCourse_UserID",
                table: "TeacherCourse",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Uploads_UploadId",
                table: "Courses",
                column: "UploadId",
                principalTable: "Uploads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Courses_CourseID",
                table: "Students",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Uploads_UploadId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Courses_CourseID",
                table: "Students");

            migrationBuilder.DropTable(
                name: "StudentContract");

            migrationBuilder.DropTable(
                name: "TeacherCourse");

            migrationBuilder.DropIndex(
                name: "IX_Students_CourseID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Courses_UploadId",
                table: "Courses");

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "86687f5a-ab0e-4dc7-a830-367329c91e8a" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "86687f5a-ab0e-4dc7-a830-367329c91e8a");

            migrationBuilder.DropColumn(
                name: "CourseID",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UploadId",
                table: "Courses");

            migrationBuilder.CreateTable(
                name: "CourseUpload",
                columns: table => new
                {
                    CoursesId = table.Column<int>(type: "int", nullable: false),
                    UploadsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseUpload", x => new { x.CoursesId, x.UploadsId });
                    table.ForeignKey(
                        name: "FK_CourseUpload_Courses_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseUpload_Uploads_UploadsId",
                        column: x => x.UploadsId,
                        principalTable: "Uploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    File = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentContracts_AspNetUsers_ParentID",
                        column: x => x.ParentID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ActiveUser", "Birthday", "Birthplace", "City", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "EmergencyPhoneNumber", "FirstName", "Gender", "Graduation", "ImageUser", "Job", "LastName", "LockoutEnabled", "LockoutEnd", "Name", "Nationality", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Street", "StreetNr", "TwoFactorEnabled", "UserName", "ZipCode" },
                values: new object[] { "8408394f-8eab-4825-bfac-fab6fc9b145a", 0, null, new DateTime(1985, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "city", "New York", "df11ac27-4372-4ee2-a3eb-44aaec05305d", "ApplicationUser", "admin@admin.com", false, null, "John", "Male", "Bachelor of Science in Computer Science", null, "Software Developer", "Doe", false, null, "admin", "American", "admin@admin.com", "ADMIN", "AQAAAAIAAYagAAAAEHrsblilNdIyg/FnWy+oRsZiJ92DHYvVNv5mEHyGbpeYI8iCBK7zipjaP9Zg/Dg22Q==", "+1234567890", false, "93b608b0-4bb6-4b75-826f-0f57481b1c6a", "123 Main St", "1A", false, "Admin", "10001" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "8408394f-8eab-4825-bfac-fab6fc9b145a" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseUpload_UploadsId",
                table: "CourseUpload",
                column: "UploadsId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentContracts_ParentID",
                table: "ParentContracts",
                column: "ParentID");
        }
    }
}
