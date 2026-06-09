using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrekBattle.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShipName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResumeCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ResumeCodeNormalized = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StateJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.SessionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_ResumeCodeNormalized",
                table: "GameSessions",
                column: "ResumeCodeNormalized",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameSessions");
        }
    }
}
