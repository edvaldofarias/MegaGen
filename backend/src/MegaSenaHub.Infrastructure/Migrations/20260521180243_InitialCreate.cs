using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaSenaHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lottery_contests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contest_number = table.Column<int>(type: "integer", nullable: false),
                    draw_date = table.Column<DateOnly>(type: "date", nullable: false),
                    accumulated = table.Column<bool>(type: "boolean", nullable: false),
                    total_prize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    source = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    combination_hash = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lottery_contests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_bets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contest_number = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    prize_won = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    checked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_bets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lottery_contest_numbers",
                columns: table => new
                {
                    contest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lottery_contest_numbers", x => new { x.contest_id, x.number });
                    table.ForeignKey(
                        name: "fk_lottery_contest_numbers_lottery_contests_contest_id",
                        column: x => x.contest_id,
                        principalTable: "lottery_contests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prize_ranges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hits = table.Column<int>(type: "integer", nullable: false),
                    winners = table.Column<int>(type: "integer", nullable: false),
                    prize_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prize_ranges", x => x.id);
                    table.ForeignKey(
                        name: "fk_prize_ranges_lottery_contests_contest_id",
                        column: x => x.contest_id,
                        principalTable: "lottery_contests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_bet_numbers",
                columns: table => new
                {
                    user_bet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_bet_numbers", x => new { x.user_bet_id, x.number });
                    table.ForeignKey(
                        name: "fk_user_bet_numbers_user_bets_user_bet_id",
                        column: x => x.user_bet_id,
                        principalTable: "user_bets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lottery_contests_combination_hash",
                table: "lottery_contests",
                column: "combination_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lottery_contests_contest_number",
                table: "lottery_contests",
                column: "contest_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lottery_contests_draw_date",
                table: "lottery_contests",
                column: "draw_date");

            migrationBuilder.CreateIndex(
                name: "ix_prize_ranges_contest_id_hits",
                table: "prize_ranges",
                columns: new[] { "contest_id", "hits" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_bets_contest_number",
                table: "user_bets",
                column: "contest_number");

            migrationBuilder.CreateIndex(
                name: "ix_user_bets_user_id",
                table: "user_bets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_bets_user_id_contest_number",
                table: "user_bets",
                columns: new[] { "user_id", "contest_number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lottery_contest_numbers");

            migrationBuilder.DropTable(
                name: "prize_ranges");

            migrationBuilder.DropTable(
                name: "user_bet_numbers");

            migrationBuilder.DropTable(
                name: "lottery_contests");

            migrationBuilder.DropTable(
                name: "user_bets");
        }
    }
}
