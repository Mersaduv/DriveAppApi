using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTypeToPhoneVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "PhoneVerifications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "PhoneVerifications");
        }
    }
}
