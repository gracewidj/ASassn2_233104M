using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASassn2_233104M.Migrations
{
    /// <inheritdoc />
    public partial class PasswordHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHistory",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                 name: "PasswordHistory",
                 table: "Users",
                 type: "nvarchar(max)",
                 nullable: false,
                 defaultValue: "[]");
        }
    }
}
