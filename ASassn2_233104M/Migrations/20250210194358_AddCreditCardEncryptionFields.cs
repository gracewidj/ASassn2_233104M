using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASassn2_233104M.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCardEncryptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreditCardIV",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardKey",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditCardIV",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreditCardKey",
                table: "Users");
        }
    }
}
