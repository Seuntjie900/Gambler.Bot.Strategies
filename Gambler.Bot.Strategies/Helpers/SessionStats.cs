using Gambler.Bot.Common.Games;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gambler.Bot.Strategies.Helpers
{
    [MoonSharp.Interpreter.MoonSharpUserData]    
    public class SessionStats
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
        [Key]
        public int SessionStatsId { get; set; }

        public long RunningTime { get; set; }

        [NotMapped]
        public TimeSpan RunningTimeSpan 
        {  
            get 
            {
                var tmp = new TimeSpan(0, 0, 0, 0, (int)RunningTime);
                return EndTime > StartTime.AddSeconds(1) ? tmp : tmp.Add(DateTime.Now - StartTime); 
            } 
        }
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
        public decimal Luck { get; set; }
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
        public decimal ProfitPerHour { get; set; }
        public decimal ProfitPer24Hour { get; set; }
        public decimal ProfitPerBet { get; set; }
        public long CurrentStreak { get { return WinStreak > LossStreak ? WinStreak : -LossStreak; } }
        public decimal MaxProfit { get; set; } = 0;
        public decimal MinProfit { get; set; } = 0;
        public decimal MaxProfitSinceReset { get; set; } = 0;
        public decimal MinProfitSinceReset { get; set; } = 0;

        public void UpdateStats(Bet newBet, bool Win)
        {
            //RunningTime = (long)(DateTime.Now - StartTime).TotalMilliseconds;
            Bets++;
            Profit += (decimal)newBet.Profit;
            if (Profit > MaxProfit)
                MaxProfit = Profit;
            if (Profit < MinProfit)
                MinProfit = Profit;
            if (Profit > MaxProfitSinceReset)
                MaxProfitSinceReset = Profit;
            if (Profit < MinProfitSinceReset)
                MinProfitSinceReset = Profit;

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
                CurrentProfit += newBet.Profit;
                ProfitSinceLastReset += newBet.Profit;
                StreakProfitSinceLastReset += newBet.Profit;
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
                
                CurrentProfit += (decimal)newBet.Profit;
                ProfitSinceLastReset += (decimal)newBet.Profit;

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
            ProfitPerBet = Profit / Bets;
            ProfitPerHour = ProfitPerBet * (Bets / ((RunningTime+(long)(DateTime.Now-StartTime).TotalMilliseconds) / 1000m / 60m / 60m));
            ProfitPer24Hour = ProfitPerHour * 24m;
        }

        private void CalculateLuck(bool Win, decimal Chance)
        {
            decimal lucktotal = (decimal)Luck * (decimal)((Wins + Losses) - 1);
            if (Win)
                lucktotal += (decimal)((decimal)100 / (decimal)Chance) * (decimal)100;
            decimal tmp = (decimal)(lucktotal / (decimal)(Wins + Losses));
            Luck = tmp;
        }

    }
}
