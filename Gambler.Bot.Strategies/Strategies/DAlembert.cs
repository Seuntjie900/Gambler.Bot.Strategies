using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Microsoft.Extensions.Logging;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games;
using System;
using Gambler.Bot.Common.Games.Limbo;

namespace Gambler.Bot.Strategies.Strategies
{
    public class DAlembert: BaseStrategy
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

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
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

            if (PreviousBet is DiceBet diceb && PreviousBet.Game== Games.Dice)
                return new PlaceDiceBet(Lastbet, High, diceb.Chance);
            if (PreviousBet is LimboBet limbob && PreviousBet.Game == Games.Limbo)
                return new PlaceLimboBet(Lastbet, limbob.Chance);
            else throw new NotImplementedException("Strategy does not support this game.");
        }

       
        public override PlaceBet RunReset(Games game)
        {
            if (game == Games.Dice)
            {
                return new PlaceDiceBet((decimal)MinBet, High, (decimal)Chance);
            }
            if (game == Games.Limbo)
            {
                return new PlaceLimboBet((decimal)MinBet, 99/(decimal)Chance);
            }
            else throw new NotImplementedException("Strategy does not support this game");
        }

        
    }
}
