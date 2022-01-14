using Microsoft.EntityFrameworkCore.Migrations;

namespace TorpedoService.Migrations
{
    public partial class idk1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Outcomes_OutcomeId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_OutcomeId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "OutcomeId",
                table: "Players");

            migrationBuilder.AddColumn<int>(
                name: "Player1Id",
                table: "Outcomes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player2Id",
                table: "Outcomes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Outcomes_Player1Id",
                table: "Outcomes",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Outcomes_Player2Id",
                table: "Outcomes",
                column: "Player2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Outcomes_Players_Player1Id",
                table: "Outcomes",
                column: "Player1Id",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Outcomes_Players_Player2Id",
                table: "Outcomes",
                column: "Player2Id",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Outcomes_Players_Player1Id",
                table: "Outcomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Outcomes_Players_Player2Id",
                table: "Outcomes");

            migrationBuilder.DropIndex(
                name: "IX_Outcomes_Player1Id",
                table: "Outcomes");

            migrationBuilder.DropIndex(
                name: "IX_Outcomes_Player2Id",
                table: "Outcomes");

            migrationBuilder.DropColumn(
                name: "Player1Id",
                table: "Outcomes");

            migrationBuilder.DropColumn(
                name: "Player2Id",
                table: "Outcomes");

            migrationBuilder.AddColumn<int>(
                name: "OutcomeId",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_OutcomeId",
                table: "Players",
                column: "OutcomeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Outcomes_OutcomeId",
                table: "Players",
                column: "OutcomeId",
                principalTable: "Outcomes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
