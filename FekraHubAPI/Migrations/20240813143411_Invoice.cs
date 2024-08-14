using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class Invoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    file = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Studentid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Students_Studentid",
                        column: x => x.Studentid,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AspNetPermissions",
                columns: new[] { "Id", "Type", "Value" },
                values: new object[] { 67, "ManageInvoice", "ManageInvoice" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoleId",
                value: "2");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetUsers", "GetUsers" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "AddUsers", "AddUsers", "4" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "AddUsers", "AddUsers" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetStudentsCourse", "GetStudentsCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateStudentsCourse", "UpdateStudentsCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetTeachersCourse", "GetTeachersCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateTeachersCourse", "UpdateTeachersCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetContracts", "GetContracts" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageEvents", "ManageEvents" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetEvents", "GetEvents" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetStudentsAttendance", "GetStudentsAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateStudentsAttendance", "UpdateStudentsAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetTeachersAttendance", "GetTeachersAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateTeachersAttendance", "UpdateTeachersAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "InsertUpdateStudentsReports", "InsertUpdateStudentsReports", "4" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "InsertUpdateStudentsReports", "InsertUpdateStudentsReports" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetStudentsReports", "GetStudentsReports", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetStudentsReports", "GetStudentsReports", "4" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetStudentsReports", "GetStudentsReports" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ApproveReports", "ApproveReports", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ApproveReports", "ApproveReports" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManagePayrolls", "ManagePayrolls" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageBooks", "ManageBooks" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "DeleteSchoolData", "DeleteSchoolData" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ResetData", "ResetData" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManagePermissions", "ManagePermissions" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageCourseSchedule", "ManageCourseSchedule" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageEventTypes", "ManageEventTypes" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageFile", "ManageFile" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "DeleteFile", "DeleteFile" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageAttendanceStatus", "ManageAttendanceStatus" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "AddStudentAttendance", "AddStudentAttendance", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "AddStudentAttendance", "AddStudentAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetCourse", "GetCourse", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetCourse", "GetCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "AddCourse", "AddCourse", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "AddCourse", "AddCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "putCourse", "putCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "DeleteCourse", "DeleteCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageStudentsToCourses", "ManageStudentsToCourses" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageLocations", "ManageLocations" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageRoom", "ManageRoom" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageExcelMigration", "ManageExcelMigration" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetEmployee", "GetEmployee", "2" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetEmployee", "GetEmployee", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetSecretary", "GetSecretary" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 48, "GetTeacher", "GetTeacher", "1" },
                    { 49, "GetParent", "GetParent", "2" },
                    { 50, "GetParent", "GetParent", "1" },
                    { 51, "UpdateUser", "UpdateUser", "1" },
                    { 52, "DeleteUser", "DeleteUser", "1" },
                    { 53, "ResetPasswordUser", "ResetPasswordUser", "1" },
                    { 54, "ManageWorkContract", "ManageWorkContract", "1" },
                    { 55, "ExportReport", "ExportReport", "1" },
                    { 56, "ManageSchoolInfo", "ManageSchoolInfo", "1" },
                    { 57, "ManageChildren", "ManageChildren", "3" },
                    { 58, "ShowParent", "ShowParent", "1" },
                    { 59, "ManageInvoice", "ManageInvoice", "1" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Studentid",
                table: "Invoices",
                column: "Studentid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoleId",
                value: "1");

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
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetStudentsCourse", "GetStudentsCourse", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateStudentsCourse", "UpdateStudentsCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetTeachersCourse", "GetTeachersCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateTeachersCourse", "UpdateTeachersCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetContracts", "GetContracts" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageEvents", "ManageEvents" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetEvents", "GetEvents" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetStudentsAttendance", "GetStudentsAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateStudentsAttendance", "UpdateStudentsAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetTeachersAttendance", "GetTeachersAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateTeachersAttendance", "UpdateTeachersAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "InsertUpdateStudentsReports", "InsertUpdateStudentsReports" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetStudentsReports", "GetStudentsReports" });

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
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManagePayrolls", "ManagePayrolls" });

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

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ResetData", "ResetData" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ManagePermissions", "ManagePermissions", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageCourseSchedule", "ManageCourseSchedule" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageEventTypes", "ManageEventTypes" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageFile", "ManageFile" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "DeleteFile", "DeleteFile" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageAttendanceStatus", "ManageAttendanceStatus" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "AddStudentAttendance", "AddStudentAttendance" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetCourse", "GetCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "AddCourse", "AddCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "putCourse", "putCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "DeleteCourse", "DeleteCourse" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageStudentsToCourses", "ManageStudentsToCourses" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ManageLocations", "ManageLocations", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageRoom", "ManageRoom" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ManageExcelMigration", "ManageExcelMigration", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetEmployee", "GetEmployee" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "GetSecretary", "GetSecretary", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetTeacher", "GetTeacher" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "GetParent", "GetParent" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "UpdateUser", "UpdateUser" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "DeleteUser", "DeleteUser" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ResetPasswordUser", "ResetPasswordUser" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ManageWorkContract", "ManageWorkContract" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ExportReport", "ExportReport" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ManageSchoolInfo", "ManageSchoolInfo", "1" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "ManageChildren", "ManageChildren", "3" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ClaimType", "ClaimValue" },
                values: new object[] { "ShowParent", "ShowParent" });
        }
    }
}
