using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Varadhi.Migrations
{
    /// <inheritdoc />
    public partial class updatedemailmessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecievedEmail",
                table: "SupportEmailMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecievedEmail",
                table: "SupportEmailMessages");
        }
    }
}
