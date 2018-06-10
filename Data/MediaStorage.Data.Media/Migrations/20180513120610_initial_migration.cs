using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MediaStorage.Data.Media.Migrations
{
    public partial class initial_migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MediaDb");

            migrationBuilder.CreateTable(
                name: "Genres",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaImageGroups",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsArchived = table.Column<bool>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaImageGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaUploads",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateSynced = table.Column<DateTime>(nullable: true),
                    DateUploaded = table.Column<DateTime>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    NumOfMedia = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaUploads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Artists",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GenreId = table.Column<Guid>(nullable: false),
                    LastUpdatedUTC = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artists_Genres_GenreId",
                        column: x => x.GenreId,
                        principalSchema: "MediaDb",
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaImages",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    Format = table.Column<string>(maxLength: 16, nullable: true),
                    ImageGroupId = table.Column<Guid>(nullable: false),
                    ImageSize = table.Column<int>(nullable: false),
                    IsArchived = table.Column<bool>(nullable: false),
                    Url = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaImages_MediaImageGroups_ImageGroupId",
                        column: x => x.ImageGroupId,
                        principalSchema: "MediaDb",
                        principalTable: "MediaImageGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    Format = table.Column<string>(maxLength: 16, nullable: true),
                    IsArchived = table.Column<bool>(nullable: false),
                    MediaInfoJson = table.Column<string>(nullable: true),
                    MediaUploadId = table.Column<Guid>(nullable: false),
                    TempUrl = table.Column<string>(maxLength: 255, nullable: true),
                    Url = table.Column<string>(maxLength: 255, nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFiles_MediaUploads_MediaUploadId",
                        column: x => x.MediaUploadId,
                        principalSchema: "MediaDb",
                        principalTable: "MediaUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ArtistId = table.Column<Guid>(nullable: false),
                    LastUpdatedUTC = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    Year = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalSchema: "MediaDb",
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistAndMediaImages",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ArtistId = table.Column<Guid>(nullable: false),
                    IsCoverImage = table.Column<bool>(nullable: false),
                    MediaImageGroupId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAndMediaImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistAndMediaImages_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalSchema: "MediaDb",
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistAndMediaImages_MediaImageGroups_MediaImageGroupId",
                        column: x => x.MediaImageGroupId,
                        principalSchema: "MediaDb",
                        principalTable: "MediaImageGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumAndMediaImages",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AlbumId = table.Column<Guid>(nullable: false),
                    IsCoverImage = table.Column<bool>(nullable: false),
                    MediaImageGroupId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumAndMediaImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumAndMediaImages_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalSchema: "MediaDb",
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumAndMediaImages_MediaImageGroups_MediaImageGroupId",
                        column: x => x.MediaImageGroupId,
                        principalSchema: "MediaDb",
                        principalTable: "MediaImageGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AlbumId = table.Column<Guid>(nullable: false),
                    DurationSec = table.Column<int>(nullable: false),
                    LastUpdatedUTC = table.Column<int>(nullable: false),
                    MediaId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    TrackNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Songs_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalSchema: "MediaDb",
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Songs_MediaFiles_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "MediaDb",
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                schema: "MediaDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ArtistId = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    SongId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalSchema: "MediaDb",
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Playlists_Songs_SongId",
                        column: x => x.SongId,
                        principalSchema: "MediaDb",
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumAndMediaImages_AlbumId",
                schema: "MediaDb",
                table: "AlbumAndMediaImages",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumAndMediaImages_MediaImageGroupId",
                schema: "MediaDb",
                table: "AlbumAndMediaImages",
                column: "MediaImageGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId",
                schema: "MediaDb",
                table: "Albums",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "Idx_Album_LastUpdated",
                schema: "MediaDb",
                table: "Albums",
                column: "LastUpdatedUTC");

            migrationBuilder.CreateIndex(
                name: "Idx_Album_Name",
                schema: "MediaDb",
                table: "Albums",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAndMediaImages_ArtistId",
                schema: "MediaDb",
                table: "ArtistAndMediaImages",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAndMediaImages_MediaImageGroupId",
                schema: "MediaDb",
                table: "ArtistAndMediaImages",
                column: "MediaImageGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_GenreId",
                schema: "MediaDb",
                table: "Artists",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "Idx_Artist_LastUpdated",
                schema: "MediaDb",
                table: "Artists",
                column: "LastUpdatedUTC");

            migrationBuilder.CreateIndex(
                name: "Idx_Artist_Name",
                schema: "MediaDb",
                table: "Artists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Format",
                schema: "MediaDb",
                table: "MediaFiles",
                column: "Format");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_MediaUploadId",
                schema: "MediaDb",
                table: "MediaFiles",
                column: "MediaUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaImages_ImageGroupId",
                schema: "MediaDb",
                table: "MediaImages",
                column: "ImageGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ArtistId",
                schema: "MediaDb",
                table: "Playlists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "Idx_Playlist_Name",
                schema: "MediaDb",
                table: "Playlists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_SongId",
                schema: "MediaDb",
                table: "Playlists",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_AlbumId",
                schema: "MediaDb",
                table: "Songs",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "Idx_Song_LastUpdated",
                schema: "MediaDb",
                table: "Songs",
                column: "LastUpdatedUTC");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_MediaId",
                schema: "MediaDb",
                table: "Songs",
                column: "MediaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumAndMediaImages",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "ArtistAndMediaImages",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "MediaImages",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "Playlists",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "MediaImageGroups",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "Songs",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "Albums",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "MediaFiles",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "Artists",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "MediaUploads",
                schema: "MediaDb");

            migrationBuilder.DropTable(
                name: "Genres",
                schema: "MediaDb");
        }
    }
}
