using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class file_uploade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Uploads_UploadsType_UploadTypeID",
                table: "Uploads");

            migrationBuilder.RenameColumn(
                name: "UploadTypeID",
                table: "Uploads",
                newName: "UploadTypeid");

            migrationBuilder.RenameIndex(
                name: "IX_Uploads_UploadTypeID",
                table: "Uploads",
                newName: "IX_Uploads_UploadTypeid");

            migrationBuilder.AddForeignKey(
                name: "FK_Uploads_UploadsType_UploadTypeid",
                table: "Uploads",
                column: "UploadTypeid",
                principalTable: "UploadsType",
                principalColumn: "Id");

            migrationBuilder.AlterColumn<byte[]>(
               name: "file",
               table: "Uploads",
               type: "varbinary(max)",
               nullable: false,
               oldClrType: typeof(byte),
               oldType: "tinyint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Uploads_UploadsType_UploadTypeid",
                table: "Uploads");

            migrationBuilder.RenameColumn(
                name: "UploadTypeid",
                table: "Uploads",
                newName: "UploadTypeID");

            migrationBuilder.RenameIndex(
                name: "IX_Uploads_UploadTypeid",
                table: "Uploads",
                newName: "IX_Uploads_UploadTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Uploads_UploadsType_UploadTypeID",
                table: "Uploads",
                column: "UploadTypeID",
                principalTable: "UploadsType",
                principalColumn: "Id");

            migrationBuilder.AlterColumn<byte>(
               name: "file",
               table: "Uploads",
               type: "tinyint",
               nullable: false,
               oldClrType: typeof(byte[]),
               oldType: "varbinary(max)");
        }
    }
}
