using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class newPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetPermissions",
                columns: new[] { "Id", "Type", "Value" },
                values: new object[,]
                {
                    { 40, "ManagePermissions", "ManagePermissions" },
                    { 41, "ManageCourseSchedule", "ManageCourseSchedule" },
                    { 42, "ManageEventTypes", "ManageEventTypes" },
                    { 43, "ManageFile", "ManageFile" },
                    { 44, "DeleteFile", "DeleteFile" },
                    { 45, "ManageAttendanceStatus", "ManageAttendanceStatus" },
                    { 46, "AddStudentAttendance", "AddStudentAttendance" },
                    { 47, "GetCourse", "GetCourse" },
                    { 48, "AddCourse", "AddCourse" },
                    { 49, "putCourse", "putCourse" },
                    { 50, "DeleteCourse", "DeleteCourse" },
                    { 51, "ManageStudentsToCourses", "ManageStudentsToCourses" },
                    { 52, "ManageLocations", "ManageLocations" },
                    { 53, "ManageRoom", "ManageRoom" },
                    { 54, "ManageExcelMigration", "ManageExcelMigration" },
                    { 55, "GetEmployee", "GetEmployee" },
                    { 56, "GetSecretary", "GetSecretary" },
                    { 57, "GetTeacher", "GetTeacher" },
                    { 58, "GetParent", "GetParent" },
                    { 59, "UpdateUser", "UpdateUser" },
                    { 60, "DeleteUser", "DeleteUser" },
                    { 61, "ResetPasswordUser", "ResetPasswordUser" },
                    { 62, "ManageWorkContract", "ManageWorkContract" },
                    { 63, "ExportReport", "ExportReport" },
                    { 64, "ManageSchoolInfo", "ManageSchoolInfo" },
                    { 65, "ManageChildren", "ManageChildren" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 21, "ManagePermissions", "ManagePermissions", "1" },
                    { 22, "ManageCourseSchedule", "ManageCourseSchedule", "1" },
                    { 23, "ManageEventTypes", "ManageEventTypes", "1" },
                    { 24, "ManageFile", "ManageFile", "1" },
                    { 25, "DeleteFile", "DeleteFile", "1" },
                    { 26, "ManageAttendanceStatus", "ManageAttendanceStatus", "1" },
                    { 27, "AddStudentAttendance", "AddStudentAttendance", "1" },
                    { 28, "GetCourse", "GetCourse", "1" },
                    { 29, "AddCourse", "AddCourse", "1" },
                    { 30, "putCourse", "putCourse", "1" },
                    { 31, "DeleteCourse", "DeleteCourse", "1" },
                    { 32, "ManageStudentsToCourses", "ManageStudentsToCourses", "1" },
                    { 33, "ManageLocations", "ManageLocations", "1" },
                    { 34, "ManageRoom", "ManageRoom", "1" },
                    { 35, "ManageExcelMigration", "ManageExcelMigration", "1" },
                    { 36, "GetEmployee", "GetEmployee", "1" },
                    { 37, "GetSecretary", "GetSecretary", "1" },
                    { 38, "GetTeacher", "GetTeacher", "1" },
                    { 39, "GetParent", "GetParent", "1" },
                    { 40, "UpdateUser", "UpdateUser", "1" },
                    { 41, "DeleteUser", "DeleteUser", "1" },
                    { 42, "ResetPasswordUser", "ResetPasswordUser", "1" },
                    { 43, "ManageWorkContract", "ManageWorkContract", "1" },
                    { 44, "ExportReport", "ExportReport", "1" },
                    { 45, "ManageSchoolInfo", "ManageSchoolInfo", "1" },
                    { 46, "ManageChildren", "ManageChildren", "3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 46);
        }
    }
}
