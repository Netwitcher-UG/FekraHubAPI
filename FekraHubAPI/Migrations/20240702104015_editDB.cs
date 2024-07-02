using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class editDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseEvents");

            migrationBuilder.DropTable(
                name: "UploadsCourse");

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CourseEvent",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int", nullable: false),
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEvent", x => new { x.EventID, x.ScheduleID });
                    table.ForeignKey(
                        name: "FK_CourseEvent_CourseSchedules_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "CourseSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseEvent_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadCourse",
                columns: table => new
                {
                    CourseID = table.Column<int>(type: "int", nullable: false),
                    UploadID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadCourse", x => new { x.CourseID, x.UploadID });
                    table.ForeignKey(
                        name: "FK_UploadCourse_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadCourse_Uploads_UploadID",
                        column: x => x.UploadID,
                        principalTable: "Uploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Type", "Value" },
                values: new object[] { "GetUsers", "GetUsers" });

            migrationBuilder.InsertData(
                table: "AspNetPermissions",
                columns: new[] { "Id", "Type", "Value" },
                values: new object[,]
                {
                    { 21, "AddUsers", "AddUsers" },
                    { 22, "GetStudentsCourse", "GetStudentsCourse" },
                    { 23, "UpdateStudentsCourse", "UpdateStudentsCourse" },
                    { 24, "GetTeachersCourse", "GetTeachersCourse" },
                    { 25, "UpdateTeachersCourse", "UpdateTeachersCourse" },
                    { 26, "GetContracts", "GetContracts" },
                    { 27, "ManageEvents", "ManageEvents" },
                    { 28, "GetEvents", "GetEvents" },
                    { 29, "GetStudentsAttendance", "GetStudentsAttendance" },
                    { 30, "UpdateStudentsAttendance", "UpdateStudentsAttendance" },
                    { 31, "GetTeachersAttendance", "GetTeachersAttendance" },
                    { 32, "UpdateTeachersAttendance", "UpdateTeachersAttendance" },
                    { 33, "InsertUpdateStudentsReports", "InsertUpdateStudentsReports" },
                    { 34, "GetStudentsReports", "GetStudentsReports" },
                    { 35, "ApproveReports", "ApproveReports" },
                    { 36, "ManagePayrolls", "ManagePayrolls" },
                    { 37, "ManageBooks", "ManageBooks" },
                    { 38, "DeleteSchoolData", "DeleteSchoolData" },
                    { 39, "ResetData", "ResetData" }
                });

            migrationBuilder.UpdateData(
                table: "SchoolInfos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UrlDomain",
                value: "http://localhost:3000");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEvent_ScheduleID",
                table: "CourseEvent",
                column: "ScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadCourse_UploadID",
                table: "UploadCourse",
                column: "UploadID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseEvent");

            migrationBuilder.DropTable(
                name: "UploadCourse");

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Students");

            migrationBuilder.CreateTable(
                name: "CourseEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventID = table.Column<int>(type: "int", nullable: true),
                    ScheduleID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseEvents_CourseSchedules_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "CourseSchedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CourseEvents_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UploadsCourse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseID = table.Column<int>(type: "int", nullable: true),
                    UploadID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadsCourse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadsCourse_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UploadsCourse_Uploads_UploadID",
                        column: x => x.UploadID,
                        principalTable: "Uploads",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetPermissions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Type", "Value" },
                values: new object[] { "ResetData", "ResetData" });

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
                    { 19, "DeleteSchoolData", "DeleteSchoolData" }
                });

            migrationBuilder.UpdateData(
                table: "SchoolInfos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UrlDomain",
                value: "http://localhost:5142");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEvents_EventID",
                table: "CourseEvents",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEvents_ScheduleID",
                table: "CourseEvents",
                column: "ScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadsCourse_CourseID",
                table: "UploadsCourse",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadsCourse_UploadID",
                table: "UploadsCourse",
                column: "UploadID");
        }
    }
}
