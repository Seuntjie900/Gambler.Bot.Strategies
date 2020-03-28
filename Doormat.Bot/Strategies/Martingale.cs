using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatBot.Strategies
{
    public class Martingale: BaseStrategy
    {
        #region Settings
        public bool EnableWinMaxMultiplier { get; set; } = false;
        public decimal WinMaxMultiplies { get; set; } = 1;
        public decimal WinMultiplier { get; set; } = 1;
        public bool EnableWinDevider { get; set; } = false;
        public int WinDevideCounter { get; set; } = 1;
        public decimal WinDevider { get; set; } = 1;
        public int WinDevidecounter { get; set; } = 1;
        public bool rdbWinReduce { get; set; } =false;
        public int StretchWin { get; set; } = 1;
        public bool EnableFirstResetWin { get; set; } = true;
        public bool EnableMK { get; set; } = false;
        public decimal MinBet { get; set; } = 0.00000100m;
        
        public bool EnableTrazel { get; set; } = false;
        public bool starthigh { get; set; } = true;
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
        public bool rdbMaxMultiplier { get; set; } = false;
        public int MaxMultiplies { get; set; } = 20;
        public decimal Multiplier { get; set; } = 2;
        public bool rdbDevider { get; set; } = false;
        public int Devidecounter { get; set; } = 10;
        public decimal Devider { get; set; } = 1;
        public bool rdbReduce { get; set; } = false;
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
        #endregion


        public override PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            decimal Lastbet = PreviousBet.TotalAmount;
            var Stats = this.Stats;
            if (Win)
            {
                if (EnableWinMaxMultiplier && Stats.WinStreak >= WinMaxMultiplies)
                {
                    WinMultiplier = 1;
                }
                else if (EnableWinDevider && Stats.WinStreak % WinDevideCounter == 1 && Stats.WinStreak > 0)
                {
                    WinMultiplier *= WinDevider;
                }
                else if (rdbWinReduce && Stats.WinStreak == WinDevidecounter && Stats.WinStreak > 0)
                {
                    WinMultiplier *= WinDevider;
                }
                if (Stats.WinStreak % StretchWin == 0)
                    Lastbet *= WinMultiplier;
                if (Stats.WinStreak == 1)
                {
                    if (EnableFirstResetWin && !EnableMK)
                    {
                        Lastbet = MinBet;
                    }
                    try
                    {
                        Chance=((decimal)BaseChance);
                    }
                    catch (Exception e)
                    {
                        Logger.DumpLog(e);
                        Logger.DumpLog(e);
                    }
                }
                if (EnableTrazel)
                {

                    High = starthigh;
                }
                if (EnableMK)
                {
                    if (decimal.Parse((Lastbet - MKDecrement).ToString("0.00000000"), System.Globalization.CultureInfo.InvariantCulture) > 0)
                    {
                        Lastbet -= MKDecrement;
                    }
                }
                if (EnableTrazel && trazelwin % TrazelWin == 0 && trazelwin != 0)
                {
                    Lastbet = trazelwinto;
                    trazelwin = -1;
                    trazelmultiply = true;
                    High = !starthigh;
                }
                else
                {
                    if (EnableTrazel)
                    {
                        Lastbet = MinBet;
                        trazelmultiply = false;
                    }
                }


                if (EnableChangeWinStreak && (Stats.WinStreak == ChangeWinStreak))
                {
                    Lastbet = ChangeWinStreakTo;
                }
                if (checkBox1)
                {
                    if (Stats.WinStreak == MutawaWins)
                        Lastbet = mutawaprev *= MutawaMultiplier;
                    if (Stats.WinStreak == MutawaWins + 1)
                    {
                        Lastbet = MinBet;
                        mutawaprev = ChangeWinStreakTo / MutawaMultiplier;
                    }

                }
                if (EnableChangeChanceWin && (Stats.WinStreak == ChangeChanceWinStreak))
                {
                    try
                    {
                        Chance = ((decimal)ChangeChanceWinTo);
                        
                    }
                    catch (Exception e)
                    {
                        Logger.DumpLog(e);
                    }
                }


            }
            else
            {
                //stop multiplying if at max or if it goes below 1
                if (rdbMaxMultiplier && Stats.LossStreak >= MaxMultiplies)
                {
                    Multiplier = 1;
                }
                else if (rdbDevider && Stats.LossStreak % Devidecounter == 0 && Stats.LossStreak > 0)
                {
                    Multiplier *= Devider;
                    if (Multiplier < 1)
                        Multiplier = 1;
                }
                //adjust multiplier according to devider
                else if (rdbReduce && Stats.LossStreak == Devidecounter && Stats.LossStreak > 0)
                {
                    Multiplier *= Devider;
                }
                if (EnableTrazel && trazelmultiply)
                {
                    Multiplier = TrazelMultiplier;
                }
                if (EnableTrazel)
                {
                    High = starthigh;
                }
                if (EnableTrazel && Stats.LossStreak + 1 >= TrazelLose && !trazelmultiply)
                {
                    Lastbet = trazelloseto;
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
                //set new bet size
                if (Stats.LossStreak % StretchLoss == 0)
                    Lastbet *= Multiplier;
                if (Stats.LossStreak == 1)
                {
                    if (EnableFirstResetLoss)
                    {
                        Lastbet = MinBet;
                    }
                }
                if (EnableMK)
                {
                    Lastbet += MKIncrement;
                }
                if (checkBox1)
                {
                    Lastbet = MinBet;
                }


                //change bet after a certain losing streak
                if (EnableChangeLoseStreak && (Stats.LossStreak == ChangeLoseStreak))
                {
                    Lastbet = ChangeLoseStreakTo;
                }
            }
            if (EnablePercentage)
            {
                Lastbet = (Percentage / 100.0m) * Balance;
            }
            return new PlaceDiceBet(Lastbet, High, (decimal)Chance);
        }

        public override PlaceDiceBet RunReset()
        {
            return new PlaceDiceBet((decimal)MinBet, High, (decimal)Chance);

        }


    }
}
