using Gambler.Bot.Common.Games.Dice;

namespace Gambler.Bot.Strategies.Strategies.Abstractions
{
    public interface iDiceStrategy
    {
        public bool High { get; set; }
        public decimal Amount { get; set; }
        public decimal Chance { get; set; }
        public decimal StartChance { get; set; }

        /// <summary>
        /// The main logic for the strategy. This is called in between every bet.
        /// </summary>
        /// <param name="PreviousBet">The bet details for the last bet that was placed</param>
        /// <returns>Bet details for the bet to be placed next.</returns>
        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win);
    }
}
