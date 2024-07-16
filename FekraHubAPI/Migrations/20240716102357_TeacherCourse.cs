using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class TeacherCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_UserId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherCourse_AspNetUsers_UserID",
                table: "TeacherCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherCourse_Courses_CourseID",
                table: "TeacherCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeacherCourse",
                table: "TeacherCourse");

            migrationBuilder.DropIndex(
                name: "IX_TeacherCourse_CourseID",
                table: "TeacherCourse");

            migrationBuilder.DropIndex(
                name: "IX_TeacherCourse_UserID",
                table: "TeacherCourse");

            migrationBuilder.DropIndex(
                name: "IX_Courses_UserId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TeacherCourse");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "TeacherCourse");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Courses");

            migrationBuilder.AlterColumn<int>(
                name: "CourseID",
                table: "TeacherCourse",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherID",
                table: "TeacherCourse",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeacherCourse",
                table: "TeacherCourse",
                columns: new[] { "CourseID", "TeacherID" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCourse_TeacherID",
                table: "TeacherCourse",
                column: "TeacherID");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherCourse_AspNetUsers_TeacherID",
                table: "TeacherCourse",
                column: "TeacherID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherCourse_Courses_CourseID",
                table: "TeacherCourse",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherCourse_AspNetUsers_TeacherID",
                table: "TeacherCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherCourse_Courses_CourseID",
                table: "TeacherCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeacherCourse",
                table: "TeacherCourse");

            migrationBuilder.DropIndex(
                name: "IX_TeacherCourse_TeacherID",
                table: "TeacherCourse");

            migrationBuilder.DropColumn(
                name: "TeacherID",
                table: "TeacherCourse");

            migrationBuilder.AlterColumn<int>(
                name: "CourseID",
                table: "TeacherCourse",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TeacherCourse",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "TeacherCourse",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Courses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeacherCourse",
                table: "TeacherCourse",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCourse_CourseID",
                table: "TeacherCourse",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCourse_UserID",
                table: "TeacherCourse",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserId",
                table: "Courses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_UserId",
                table: "Courses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherCourse_AspNetUsers_UserID",
                table: "TeacherCourse",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherCourse_Courses_CourseID",
                table: "TeacherCourse",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
