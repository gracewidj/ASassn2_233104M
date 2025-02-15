using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASassn2_233104M.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
    name: "Users",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        FullName = table.Column<string>(nullable: false),
        Gender = table.Column<string>(nullable: false),
        MobileNo = table.Column<string>(nullable: false),
        DeliveryAddress = table.Column<string>(nullable: false),
        Email = table.Column<string>(maxLength: 255, nullable: false),
        Password = table.Column<string>(nullable: false),
        PasswordSalt = table.Column<string>(nullable: true),
        CreditCardNumber = table.Column<string>(nullable: true),
        AboutMe = table.Column<string>(nullable: true),
        Photo = table.Column<byte[]>(nullable: true),
        SessionToken = table.Column<string>(nullable: true),
        FailedLoginAttempts = table.Column<int>(nullable: false, defaultValue: 0),
        LockoutEnd = table.Column<DateTime>(nullable: true),
        PasswordHistoryy = table.Column<string>(nullable: false, defaultValue: "[]"),
        LastPasswordChange = table.Column<DateTime>(nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Users", x => x.Id);
    });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
