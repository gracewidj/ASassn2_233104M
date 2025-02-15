using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASassn2_233104M.Migrations
{
    /// <inheritdoc />
    public partial class ReapplyPasswordTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHistory",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChange",
                table: "Users",
                type: "datetime2",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
