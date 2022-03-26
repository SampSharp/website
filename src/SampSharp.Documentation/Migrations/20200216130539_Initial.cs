using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SampSharp.Documentation.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocVersions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Uri = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocAssets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uri = table.Column<string>(nullable: true),
                    VersionId = table.Column<int>(nullable: true),
                    Data = table.Column<byte[]>(type: "mediumblob", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocAssets_DocVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "DocVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Uri = table.Column<string>(nullable: true),
                    VersionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocCategories_DocVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "DocVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocArticles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    RedirectUrl = table.Column<string>(nullable: true),
                    RedirectPage = table.Column<string>(nullable: true),
                    LastModification = table.Column<DateTime>(nullable: false),
                    EditUrl = table.Column<string>(nullable: true),
                    Introduction = table.Column<string>(type: "longtext", nullable: true),
                    Uri = table.Column<string>(nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    CategoryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocArticles_DocCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "DocCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocParagraphs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ArticleId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Uri = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocParagraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocParagraphs_DocArticles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "DocArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocArticles_CategoryId",
                table: "DocArticles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DocAssets_VersionId",
                table: "DocAssets",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocCategories_VersionId",
                table: "DocCategories",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocParagraphs_ArticleId",
                table: "DocParagraphs",
                column: "ArticleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocAssets");

            migrationBuilder.DropTable(
                name: "DocParagraphs");

            migrationBuilder.DropTable(
                name: "DocArticles");

            migrationBuilder.DropTable(
                name: "DocCategories");

            migrationBuilder.DropTable(
                name: "DocVersions");
        }
    }
}
