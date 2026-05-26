using Gambler.Bot.Strategies.Strategies.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Twist;

namespace Gambler.Bot.Strategies.Strategies
{
    public class Martingale: BaseStrategy
    {

        public override string StrategyName { get; protected set; } = "Martingale";
        #region Settings
        public MartingaleMultiplierMode WinMultiplierMode { get; set; }
        public decimal WinMaxMultiplies { get; set; } = 1;
        public decimal WinMultiplier { get; private set; } = 1;
        public decimal WinBaseMultiplier { get; set; } = 1;
        public int WinDevideCounter { get; set; } = 1;
        public decimal WinDevider { get; set; } = 1;
        public int WinDevidecounter { get; set; } = 1;   
        public int StretchWin { get; set; } = 1;
        public bool EnableFirstResetWin { get; set; } = true;
        public bool EnableMK { get; set; } = false;
        public decimal MinBet { get; set; } = 0.00000100m;
        
        public bool EnableTrazel { get; set; } = false;
        public bool starthigh { get; set; } = true;
        public bool startlow { get { return !starthigh; } set { starthigh = !value; } }
        public decimal MKDecrement { get; set; } = 1;
        public decimal trazelwin { get; set; } = 1;
        public decimal TrazelWin { get; set; } = 1;
        public decimal trazelwinto { get; set; } = 1;
        public bool trazelmultiply { get; set; } = false;
        public bool EnableChangeWinStreak { get; set; } = false;
        public int ChangeWinStreak { get; set; } = 1;
        public decimal ChangeWinStreakTo { get; set; } = 49.5m;
        public bool checkBox1 { get; set; } = false;//??wtf is this????
        public int MutawaWins { get; set; } = 1;
        public decimal mutawaprev { get; set; } = 1;
        public decimal MutawaMultiplier { get; set; } = 49.5m;
        public int ChangeChanceWinStreak { get; set; } = 10;
        public bool EnableChangeChanceWin { get; set; } = false;
        public decimal ChangeChanceWinTo { get; set; } = 90;
        public bool EnableChangeChanceLose { get; set; } = false;
        public int ChangeChanceLoseStreak { get; set; } = 10;
        public decimal ChangeChanceLoseTo { get; set; } = 90;
        public bool rdbMaxMultiplier { get; set; } = false;
        public int MaxMultiplies { get; set; } = 20;
        public decimal Multiplier { get; private set; } = 2;
        public decimal BaseMultiplier { get; set; } = 2;
        public MartingaleMultiplierMode MultiplierMode { get; set; } = 0;
        public int Devidecounter { get; set; } = 10;
        public decimal Devider { get; set; } = 1;        
        public decimal TrazelMultiplier { get; set; } = 1;
        public int TrazelLose { get; set; } = 1;
        public decimal trazelloseto { get; set; } = 1;
        public int StretchLoss { get; set; } = 1;
        public bool EnableFirstResetLoss { get; set; } = false;
        public decimal MKIncrement { get; set; } = 1;
        public int ChangeLoseStreak { get; set; } = 1;
        public bool EnableChangeLoseStreak { get; set; } = false;
        public decimal ChangeLoseStreakTo { get; set; } = 1;
        public bool EnablePercentage { get; set; }= false;
        public decimal Percentage { get; set; } = 0.1m;
        public decimal BaseChance { get; set; } = 49.5m;
        public bool High { get ; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set ; }
        #endregion

       

        public Martingale(ILogger logger) : base(logger)
        {

        }
        public Martingale()
        {
            
        }

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
        {
            decimal lastBet = PreviousBet.TotalAmount;
            var stats = Stats;

            lastBet = Win
                ? CalculateWinBet(lastBet, stats)
                : CalculateLossBet(lastBet, stats);

            if (EnablePercentage)
            {
                lastBet = (Percentage / 100.0m) * Balance;
            }

            return CreatePlaceBet(PreviousBet, lastBet);
        }

        protected virtual decimal CalculateWinBet(decimal lastBet, Helpers.SessionStats stats)
        {
            if (WinMultiplierMode == MartingaleMultiplierMode.Variable && stats.WinStreak % WinDevidecounter == 0 && stats.WinStreak > 0)
            {
                WinMultiplier *= WinDevider;
            }
            else if (WinMultiplierMode == MartingaleMultiplierMode.ChangeOnce && stats.WinStreak % WinDevideCounter == 1 && stats.WinStreak > 0)
            {
                WinMultiplier *= WinDevider;
            }
            else if (WinMultiplierMode == MartingaleMultiplierMode.Max && stats.WinStreak == WinMaxMultiplies && stats.WinStreak > 0)
            {
                WinMultiplier = 1;
            }

            if (stats.WinStreak % StretchWin == 0)
                lastBet *= WinMultiplier;

            if (stats.WinStreak == 1)
            {
                if (EnableFirstResetWin && !EnableMK)
                {
                    lastBet = MinBet;
                }

                Chance = BaseChance;
            }

            if (EnableTrazel)
            {
                High = starthigh;
            }

            if (EnableMK)
            {
                if (decimal.Parse((lastBet - MKDecrement).ToString("0.00000000"), System.Globalization.CultureInfo.InvariantCulture) > 0)
                {
                    lastBet -= MKDecrement;
                }
            }

            if (EnableTrazel && trazelwin % TrazelWin == 0 && trazelwin != 0)
            {
                lastBet = trazelwinto;
                trazelwin = -1;
                trazelmultiply = true;
                High = !starthigh;
            }
            else if (EnableTrazel)
            {
                lastBet = MinBet;
                trazelmultiply = false;
            }

            if (EnableChangeWinStreak && stats.WinStreak == ChangeWinStreak)
            {
                lastBet = ChangeWinStreakTo;
            }

            if (checkBox1)
            {
                if (stats.WinStreak == MutawaWins)
                    lastBet = mutawaprev *= MutawaMultiplier;

                if (stats.WinStreak == MutawaWins + 1)
                {
                    lastBet = MinBet;
                    mutawaprev = ChangeWinStreakTo / MutawaMultiplier;
                }
            }

            if (EnableChangeChanceWin && stats.WinStreak == ChangeChanceWinStreak)
            {
                Chance = ChangeChanceWinTo;
            }

            return lastBet;
        }

        protected virtual decimal CalculateLossBet(decimal lastBet, Helpers.SessionStats stats)
        {
            if (MultiplierMode == MartingaleMultiplierMode.Variable && stats.LossStreak % Devidecounter == 0 && stats.LossStreak > 0)
            {
                Multiplier *= Devider;
            }
            else if (MultiplierMode == MartingaleMultiplierMode.ChangeOnce && stats.LossStreak % Devidecounter == 0 && stats.LossStreak > 0)
            {
                Multiplier *= Devider;
                if (Multiplier < 1)
                    Multiplier = 1;
            }
            else if (MultiplierMode == MartingaleMultiplierMode.Max && stats.LossStreak == MaxMultiplies && stats.LossStreak > 0)
            {
                Multiplier = 1;
            }

            if (EnableTrazel && trazelmultiply)
            {
                Multiplier = TrazelMultiplier;
            }

            if (EnableTrazel)
            {
                High = starthigh;
            }

            if (EnableTrazel && stats.LossStreak + 1 >= TrazelLose && !trazelmultiply)
            {
                lastBet = trazelloseto;
                trazelmultiply = true;
                High = !starthigh;
            }

            if (trazelmultiply)
            {
                trazelwin = -1;
            }
            else
            {
                trazelwin = 0;
            }

            if (stats.LossStreak % StretchLoss == 0)
                lastBet *= Multiplier;

            if (stats.LossStreak == 1 && EnableFirstResetLoss)
            {
                lastBet = MinBet;
            }

            if (EnableMK)
            {
                lastBet += MKIncrement;
            }

            if (checkBox1)
            {
                lastBet = MinBet;
            }

            if (EnableChangeLoseStreak && stats.LossStreak == ChangeLoseStreak)
            {
                lastBet = ChangeLoseStreakTo;
            }

            if (EnableChangeChanceLose && stats.WinStreak == ChangeChanceLoseStreak)
            {
                Chance = ChangeChanceLoseTo;
            }

            return lastBet;
        }

        protected virtual PlaceBet CreatePlaceBet(Bet previousBet, decimal amount)
        {
            if (previousBet is DiceBet && previousBet.Game == Games.Dice)
                return new PlaceDiceBet(amount, High, Chance);

            if (previousBet is LimboBet && previousBet.Game == Games.Limbo)
                return new PlaceLimboBet(amount, (100 - Config.Edge) / Chance);

            if (previousBet is TwistBet twistBet && previousBet.Game == Games.Twist)
                return new PlaceTwistBet(amount, High, twistBet.Chance);

            if (previousBet is CrashBet crashBet && previousBet.Game == Games.Crash)
                return new PlaceCrashBet(amount, crashBet.Payout);

            throw new NotImplementedException("Strategy does not support this game.");
        }

        public override PlaceBet RunReset(Games Game)
        {
            Amount = MinBet;
            High = starthigh;
            Chance = BaseChance;
            Multiplier = BaseMultiplier;
            WinMultiplier = WinBaseMultiplier;
            if (Game == Games.Dice)
            {
                return new PlaceDiceBet((decimal)MinBet, High, (decimal)Chance);
            }
            if (Game == Games.Limbo)
            {
                return new PlaceLimboBet((decimal)MinBet, (100-Config.Edge) / (decimal)Chance);
            }
            if (Game == Games.Twist)
            {
                return new PlaceTwistBet((decimal)MinBet, High, Chance);
            }            
            if (Game == Games.Crash)
            {
                return new PlaceCrashBet((decimal)MinBet, Chance);
            }
            else throw new NotImplementedException("Strategy does not support this game");
        }

     
    }

    public enum MartingaleMultiplierMode
    {
        Constant = 0, Variable =1, ChangeOnce=2, Max=3
    }
}
