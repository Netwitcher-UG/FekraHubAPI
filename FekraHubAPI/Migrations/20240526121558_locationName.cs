using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class locationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Location",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "b65a9d3d-d70c-4083-b84d-a241533e299d" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b65a9d3d-d70c-4083-b84d-a241533e299d");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Location");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ActiveUser", "Birthday", "Birthplace", "City", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "EmergencyPhoneNumber", "FirstName", "Gender", "Graduation", "ImageUser", "Job", "LastName", "LockoutEnabled", "LockoutEnd", "Name", "Nationality", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Street", "StreetNr", "TwoFactorEnabled", "UserName", "ZipCode" },
                values: new object[] { "d9e8a9de-3a7e-48ea-b4e6-238105f1e7be", 0, null, new DateTime(1985, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "city", "New York", "90771aad-ebc5-4570-a07c-3829db5768be", "ApplicationUser", "admin@admin.com", false, null, "John", "Male", "Bachelor of Science in Computer Science", null, "Software Developer", "Doe", false, null, "admin", "American", "admin@admin.com", "ADMIN", "AQAAAAIAAYagAAAAEH52HfBoffdWK0MWh69f0bYd6kUgjgN6FXB2SgJrC49KpZuHR419Vq1JOEj0Zqy3yA==", "+1234567890", false, "126d6733-26be-4845-85cc-186d60472cf0", "123 Main St", "1A", false, "Admin", "10001" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "d9e8a9de-3a7e-48ea-b4e6-238105f1e7be" });
        }
    }
}
