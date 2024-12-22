using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Varadhi.Migrations
{
    /// <inheritdoc />
    public partial class changesinemailmessagetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TicketId",
                table: "SupportEmailMessages",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "SupportEmailMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailMessageId",
                table: "SupportEmailMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "SupportEmailMessages");

            migrationBuilder.DropColumn(
                name: "EmailMessageId",
                table: "SupportEmailMessages");

            migrationBuilder.AlterColumn<string>(
                name: "TicketId",
                table: "SupportEmailMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
