using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gambler.Bot.AutoBet.Migrations
{
    /// <inheritdoc />
    public partial class MigratedToEf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrashBets",
                columns: table => new
                {
                    BetID = table.Column<string>(type: "TEXT", nullable: false),
                    Payout = table.Column<decimal>(type: "TEXT", nullable: false),
                    Crash = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Date = table.Column<decimal>(type: "TEXT", nullable: false),
                    Profit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Userid = table.Column<long>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true),
                    Edge = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrashBets", x => x.BetID);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Icon = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "DiceBets",
                columns: table => new
                {
                    BetID = table.Column<string>(type: "TEXT", nullable: false),
                    Roll = table.Column<decimal>(type: "TEXT", nullable: false),
                    High = table.Column<bool>(type: "INTEGER", nullable: false),
                    Chance = table.Column<decimal>(type: "TEXT", nullable: false),
                    Nonce = table.Column<long>(type: "INTEGER", nullable: false),
                    ServerHash = table.Column<string>(type: "TEXT", nullable: true),
                    ServerSeed = table.Column<string>(type: "TEXT", nullable: true),
                    ClientSeed = table.Column<string>(type: "TEXT", nullable: true),
                    WinnableType = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Date = table.Column<decimal>(type: "TEXT", nullable: false),
                    Profit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Userid = table.Column<long>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true),
                    Edge = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiceBets", x => x.BetID);
                });

            migrationBuilder.CreateTable(
                name: "PlinkoBets",
                columns: table => new
                {
                    BetID = table.Column<string>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Date = table.Column<decimal>(type: "TEXT", nullable: false),
                    Profit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Userid = table.Column<long>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true),
                    Edge = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlinkoBets", x => x.BetID);
                });

            migrationBuilder.CreateTable(
                name: "RouletteBets",
                columns: table => new
                {
                    BetID = table.Column<string>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Date = table.Column<decimal>(type: "TEXT", nullable: false),
                    Profit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Userid = table.Column<long>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true),
                    Edge = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouletteBets", x => x.BetID);
                });

            migrationBuilder.CreateTable(
                name: "Seeds",
                columns: table => new
                {
                    ServerHash = table.Column<string>(type: "TEXT", nullable: false),
                    ClientSeed = table.Column<string>(type: "TEXT", nullable: true),
                    ServerSeed = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousServer = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousClient = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousHash = table.Column<string>(type: "TEXT", nullable: true),
                    Nonce = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seeds", x => x.ServerHash);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionStatsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Simulation = table.Column<bool>(type: "INTEGER", nullable: false),
                    RunningTime = table.Column<long>(type: "INTEGER", nullable: false),
                    Losses = table.Column<long>(type: "INTEGER", nullable: false),
                    Wins = table.Column<long>(type: "INTEGER", nullable: false),
                    Bets = table.Column<long>(type: "INTEGER", nullable: false),
                    LossStreak = table.Column<long>(type: "INTEGER", nullable: false),
                    WinStreak = table.Column<long>(type: "INTEGER", nullable: false),
                    Profit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Wagered = table.Column<decimal>(type: "TEXT", nullable: false),
                    WorstStreak = table.Column<long>(type: "INTEGER", nullable: false),
                    WorstStreak3 = table.Column<long>(type: "INTEGER", nullable: false),
                    WorstStreak2 = table.Column<long>(type: "INTEGER", nullable: false),
                    BestStreak = table.Column<long>(type: "INTEGER", nullable: false),
                    BestStreak3 = table.Column<long>(type: "INTEGER", nullable: false),
                    BestStreak2 = table.Column<long>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    laststreaklose = table.Column<long>(type: "INTEGER", nullable: false),
                    laststreakwin = table.Column<long>(type: "INTEGER", nullable: false),
                    LargestBet = table.Column<decimal>(type: "TEXT", nullable: false),
                    LargestLoss = table.Column<decimal>(type: "TEXT", nullable: false),
                    LargestWin = table.Column<decimal>(type: "TEXT", nullable: false),
                    Luck = table.Column<decimal>(type: "TEXT", nullable: false),
                    AvgWin = table.Column<decimal>(type: "TEXT", nullable: false),
                    AvgLoss = table.Column<decimal>(type: "TEXT", nullable: false),
                    AvgStreak = table.Column<decimal>(type: "TEXT", nullable: false),
                    CurrentProfit = table.Column<decimal>(type: "TEXT", nullable: false),
                    StreakProfitSinceLastReset = table.Column<decimal>(type: "TEXT", nullable: false),
                    StreakLossSinceLastReset = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProfitSinceLastReset = table.Column<decimal>(type: "TEXT", nullable: false),
                    winsAtLastReset = table.Column<long>(type: "INTEGER", nullable: false),
                    NumLossStreaks = table.Column<long>(type: "INTEGER", nullable: false),
                    NumWinStreaks = table.Column<long>(type: "INTEGER", nullable: false),
                    NumStreaks = table.Column<long>(type: "INTEGER", nullable: false),
                    PorfitSinceLimitAction = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProfitPerHour = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProfitPer24Hour = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProfitPerBet = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxProfit = table.Column<decimal>(type: "TEXT", nullable: false),
                    MinProfit = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxProfitSinceReset = table.Column<decimal>(type: "TEXT", nullable: false),
                    MinProfitSinceReset = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionStatsId);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    edge = table.Column<decimal>(type: "TEXT", nullable: false),
                    maxroll = table.Column<decimal>(type: "TEXT", nullable: false),
                    cantip = table.Column<bool>(type: "INTEGER", nullable: false),
                    tipusingname = table.Column<bool>(type: "INTEGER", nullable: false),
                    canwithdraw = table.Column<bool>(type: "INTEGER", nullable: false),
                    canresetseed = table.Column<bool>(type: "INTEGER", nullable: false),
                    caninvest = table.Column<bool>(type: "INTEGER", nullable: false),
                    siteurl = table.Column<string>(type: "TEXT", nullable: true),
                    Currencies = table.Column<string>(type: "TEXT", nullable: true),
                    Games = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Sitename = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Sites_Sitename",
                        column: x => x.Sitename,
                        principalTable: "Sites",
                        principalColumn: "name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Sitename",
                table: "Users",
                column: "Sitename");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrashBets");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "DiceBets");

            migrationBuilder.DropTable(
                name: "PlinkoBets");

            migrationBuilder.DropTable(
                name: "RouletteBets");

            migrationBuilder.DropTable(
                name: "Seeds");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
