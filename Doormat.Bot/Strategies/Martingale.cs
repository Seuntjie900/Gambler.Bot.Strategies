using Gambler.Bot.Core.Games;
using Gambler.Bot.Core.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.AutoBet.Strategies
{
    public class Martingale: BaseStrategy, iDiceStrategy
    {
        public override string StrategyName { get; protected set; } = "Martingale";
        #region Settings
        public int WinMultiplierMode { get; set; }
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
        public int MultiplierMode { get; set; } = 0;
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
        public decimal StartChance { get ; set ; }
        #endregion

        public Martingale(ILogger logger) : base(logger)
        {

        }
        public Martingale()
        {
            
        }

        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            decimal Lastbet = PreviousBet.TotalAmount;
            var Stats = this.Stats;
            if (Win)
            {
                if (WinMultiplierMode==1 && Stats.WinStreak >= WinMaxMultiplies)
                {
                    WinMultiplier = 1;
                }
                else if (WinMultiplierMode==2 && Stats.WinStreak % WinDevideCounter == 1 && Stats.WinStreak > 0)
                {
                    WinMultiplier *= WinDevider;
                }
                else if (WinMultiplierMode == 3 && Stats.WinStreak == WinDevidecounter && Stats.WinStreak > 0)

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
                        _Logger?.LogError(e.ToString());                        
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
                        _Logger?.LogError(e.ToString());
                    }
                }


            }
            else
            {
                //stop multiplying if at max or if it goes below 1

                if (MultiplierMode==1 && Stats.LossStreak >= MaxMultiplies)
                {
                    Multiplier = 1;
                }
                else if (MultiplierMode==2 && Stats.LossStreak % Devidecounter == 0 && Stats.LossStreak > 0)
                {
                    Multiplier *= Devider;
                    if (Multiplier < 1)
                        Multiplier = 1;
                }
                //adjust multiplier according to devider

                else if (MultiplierMode == 3 && Stats.LossStreak == Devidecounter && Stats.LossStreak > 0)
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
                if (EnableChangeChanceLose && (Stats.WinStreak == ChangeChanceLoseStreak))
                {
                    try
                    {
                        Chance = ((decimal)ChangeChanceLoseTo);

                    }
                    catch (Exception e)
                    {
                        _Logger?.LogError(e.ToString());
                    }
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
            Amount = MinBet;
            High = starthigh;
            Chance = BaseChance;
            Multiplier = BaseMultiplier;
            WinMultiplier = WinBaseMultiplier;
            return new PlaceDiceBet((decimal)MinBet, starthigh, (decimal)Chance);
        }


    }
}
