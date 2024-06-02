using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.Common.Games;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.Strategies.Strategies
{
    public class DAlembert: BaseStrategy, iDiceStrategy
    {
        public override string StrategyName { get; protected set; } = "D'Alembert";
        public int AlembertStretchWin { get; set; } = 0;

        public int AlembertStretchLoss { get; set; } = 0;

        public decimal AlembertIncrementLoss { get; set; } = 0.00000100m;

        public decimal MinBet { get; set; } = 0.00000100m;

        public decimal AlembertIncrementWin { get; set; } = 0.00000100m;
        public bool High { get; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set; }
        public decimal StartChance { get; set; }

        public DAlembert(ILogger logger):base(logger)
        {
            
        }

        public DAlembert()
        {
            
        }

        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
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
