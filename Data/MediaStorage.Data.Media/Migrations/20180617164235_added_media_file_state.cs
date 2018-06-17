using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MediaStorage.Data.Media.Migrations
{
    public partial class added_media_file_state : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaFileStateInfos",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    MediaFileId = table.Column<Guid>(nullable: false),
                    StateInfoJson = table.Column<string>(maxLength: 4096, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFileStateInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFileStateInfos_MediaFiles_MediaFileId",
                        column: x => x.MediaFileId,
                        principalSchema: "MediaDb",
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileStateInfos_MediaFileId",
                schema: "MediaDb",
                table: "MediaFileStateInfos",
                column: "MediaFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaFileStateInfos",
                schema: "MediaDb");
        }
    }
}
