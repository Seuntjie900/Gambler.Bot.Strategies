using DoormatCore.Games;
using DoormatCore.Storage;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DoormatCore.Helpers
{
    [MoonSharp.Interpreter.MoonSharpUserData]
    [PersistentTableName("SESSIONSTATS")]
    public class SessionStats:PersistentBase
    {
        public bool Simulation { get; set; }
        public SessionStats()
        {
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            RunningTime = 0;
            Simulation = false;
        }
        public SessionStats(bool Simulation)
        {
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            RunningTime = 0;
            this.Simulation = Simulation;
        }
        public long RunningTime { get; set; }
        public long Losses { get; set; }
        public long Wins { get; set; }
        public long Bets { get; set; }
        public long LossStreak { get; set; }
        public long WinStreak { get; set; }
        public decimal Profit { get; set; }
        public decimal Wagered { get; set; }
        public long WorstStreak { get; set; }
        public long WorstStreak3 { get; set; }
        public long WorstStreak2 { get; set; }
        public long BestStreak { get; set; }
        public long BestStreak3 { get; set; }
        public long BestStreak2 { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long laststreaklose { get; set; }
        public long laststreakwin { get; set; }
        public decimal LargestBet { get; set; }
        public decimal LargestLoss { get; set; }
        public decimal LargestWin { get; set; }
        public decimal luck { get; set; }
        public decimal AvgWin { get; set; }        
        public decimal AvgLoss { get; set; }
        public decimal AvgStreak { get; set; }
        public decimal CurrentProfit { get; set; }
        public decimal StreakProfitSinceLastReset { get; set; }
        public decimal StreakLossSinceLastReset { get; set; }
        public decimal ProfitSinceLastReset { get; set; }
        public long winsAtLastReset { get; set; }
        public long NumLossStreaks { get; set; }
        public long NumWinStreaks { get; set; }
        public long NumStreaks { get; set; }
        public decimal PorfitSinceLimitAction { get; set; }
        public void UpdateStats(Bet newBet, bool Win)
        {
            Bets++;
            Profit += (decimal)newBet.Profit;
            Wagered += (decimal)newBet.TotalAmount;
            PorfitSinceLimitAction += (decimal)newBet.Profit;
            if (Win)
            {
                if (LargestWin < (decimal) newBet.Profit)
                    LargestWin = (decimal)newBet.Profit;
            }
            else
            {
                if (LargestLoss < (decimal)-newBet.Profit)
                    LargestLoss = (decimal)-newBet.Profit;
            }

            if (LargestBet < (decimal)newBet.TotalAmount)
                LargestBet = (decimal)newBet.TotalAmount;
            if (Win)
            {
                if (WinStreak == 0)
                {
                    CurrentProfit = 0;
                    StreakProfitSinceLastReset = 0;
                    StreakLossSinceLastReset = 0;
                }
                CurrentProfit += Profit;
                ProfitSinceLastReset += Profit;
                StreakProfitSinceLastReset += Profit;
                Wins++;
                WinStreak++;
                if (LossStreak != 0)
                {
                    decimal avglosecalc = AvgLoss * NumLossStreaks;
                    avglosecalc += LossStreak;
                    avglosecalc /= ++NumLossStreaks;
                    AvgLoss = avglosecalc;
                    decimal avgbetcalc = AvgStreak * NumStreaks;
                    avgbetcalc -= LossStreak;
                    avgbetcalc /= ++NumStreaks;
                    AvgStreak = avgbetcalc;
                    if (LossStreak > WorstStreak3)
                    {
                        WorstStreak3 = LossStreak;
                        if (LossStreak > WorstStreak2)
                        {
                            WorstStreak3 = WorstStreak2;
                            WorstStreak2 = LossStreak;
                            if (LossStreak > WorstStreak)
                            {
                                WorstStreak2 = WorstStreak;
                                WorstStreak = LossStreak;
                            }
                        }
                    }
                }
                LossStreak = 0;
            }
            else if (!Win)
            {
                if (LossStreak == 0)
                {
                    CurrentProfit = 0;
                    StreakProfitSinceLastReset = 0;
                    StreakLossSinceLastReset = 0;
                }
                CurrentProfit -= (decimal)newBet.TotalAmount;
                ProfitSinceLastReset -= (decimal)newBet.TotalAmount;

                StreakLossSinceLastReset -= (decimal)newBet.TotalAmount;
                Losses++;
                LossStreak++;

                if (WinStreak != 0)
                {
                    decimal avgwincalc = AvgWin * NumWinStreaks;
                    avgwincalc += WinStreak;
                    avgwincalc /= ++NumWinStreaks;
                    AvgWin = avgwincalc;
                    decimal avgbetcalc = AvgStreak * NumStreaks;
                    avgbetcalc += WinStreak;
                    avgbetcalc /= ++NumStreaks;
                    AvgStreak = avgbetcalc;
                    if (WinStreak > BestStreak3)
                    {
                        BestStreak3 = WinStreak;
                        if (WinStreak > BestStreak2)
                        {
                            BestStreak3 = BestStreak2;
                            BestStreak2 = WinStreak;
                            if (WinStreak > BestStreak)
                            {
                                BestStreak2 = BestStreak;
                                BestStreak = WinStreak;
                            }
                        }
                    }
                }
                //reset win streak
                WinStreak = 0;                
            }
            //CalculateLuck(Win, (decimal)newBet.Chance);
        }

        private void CalculateLuck(bool Win, decimal Chance)
        {
            decimal lucktotal = (decimal)luck * (decimal)((Wins + Losses) - 1);
            if (Win)
                lucktotal += (decimal)((decimal)100 / (decimal)Chance) * (decimal)100;
            decimal tmp = (decimal)(lucktotal / (decimal)(Wins + Losses));
            luck = tmp;
        }

    }
}
