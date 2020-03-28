using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatBot.Strategies
{
    public class DAlembert: BaseStrategy
    {

        public int AlembertStretchWin { get; set; } = 1;

        public int AlembertStretchLoss { get; set; } = 1;

        public decimal AlembertIncrementLoss { get; set; } = 0.00000100m;

        public decimal MinBet { get; set; } = 0.00000100m;

        public decimal AlembertIncrementWin { get; set; } = 0.00000100m;

        public override PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            decimal Lastbet = PreviousBet.TotalAmount;
            SessionStats Stats = this.Stats;
            if (Win)
            {
                
                if ((Stats.WinStreak) % (AlembertStretchWin +1) == 0)
                {
                    Lastbet += AlembertIncrementWin;
                }
            }
            else
            {
                if ((Stats.LossStreak) % (AlembertStretchLoss + 1) == 0)
                {
                    Lastbet += AlembertIncrementLoss;
                }
            }
            if (Lastbet < MinBet)
                Lastbet = MinBet;

            return new PlaceDiceBet(Lastbet, High, PreviousBet.Chance);
        }

       
        public override PlaceDiceBet RunReset()
        {
            return new PlaceDiceBet((decimal)MinBet, High, (decimal)Chance);
                
        }

        
    }
}
