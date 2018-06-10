using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MediaStorage.Data.Streaming.Migrations
{
    public partial class initial_migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "StreamingDb");

            migrationBuilder.CreateTable(
                name: "StreamingUsers",
                schema: "StreamingDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateLastActivity = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(maxLength: 128, nullable: true),
                    PasswordSalt = table.Column<string>(maxLength: 128, nullable: true),
                    UserName = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamingUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StreamingSessions",
                schema: "StreamingDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateFinished = table.Column<DateTime>(nullable: true),
                    DateStarted = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    PlayingAtMSec = table.Column<int>(nullable: false),
                    PlayingMediaId = table.Column<Guid>(nullable: true),
                    StateInfoJson = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false),
                    UserIp = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamingSessions_StreamingUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "StreamingDb",
                        principalTable: "StreamingUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_StreamingSession_User",
                schema: "StreamingDb",
                table: "StreamingSessions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamingUsers_UserName",
                schema: "StreamingDb",
                table: "StreamingUsers",
                column: "UserName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreamingSessions",
                schema: "StreamingDb");

            migrationBuilder.DropTable(
                name: "StreamingUsers",
                schema: "StreamingDb");
        }
    }
}
