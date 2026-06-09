using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPacijentDatotekaUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PacijentDatoteke",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacijentId = table.Column<int>(type: "int", nullable: false),
                    OriginalnoIme = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    NazivNaDisku = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    Putanja = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Velicina = table.Column<long>(type: "bigint", nullable: false),
                    DatumUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacijentDatoteke", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PacijentDatoteke_Patients_PacijentId",
                        column: x => x.PacijentId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PacijentDatoteke_PacijentId",
                table: "PacijentDatoteke",
                column: "PacijentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PacijentDatoteke");
        }
    }
}
