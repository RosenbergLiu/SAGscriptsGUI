using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASKOmaster.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    idoss = table.Column<string>(type: "text", nullable: false),
                    wos = table.Column<string>(type: "text", nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tech = table.Column<string>(type: "text", nullable: true),
                    parts = table.Column<string[]>(type: "text[]", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    art = table.Column<string>(type: "text", nullable: true),
                    warranty = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.idoss);
                });

            migrationBuilder.CreateTable(
                name: "Release",
                columns: table => new
                {
                    sap = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    part = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    dn = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Release", x => x.sap);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Release");
        }
    }
}
