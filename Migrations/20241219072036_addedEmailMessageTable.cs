using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Varadhi.Migrations
{
    /// <inheritdoc />
    public partial class addedEmailMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportEmailMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InternetMessageId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyPreview = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConversationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasAttachments = table.Column<bool>(type: "bit", nullable: false),
                    Importance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ParentFolderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportEmailMessages", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportEmailMessages");
        }
    }
}
