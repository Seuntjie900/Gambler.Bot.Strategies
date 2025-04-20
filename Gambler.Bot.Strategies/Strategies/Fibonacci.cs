using Gambler.Bot.Strategies.Strategies.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Limbo;

namespace Gambler.Bot.Strategies.Strategies
{
    public class Fibonacci: BaseStrategy
    {
        public override string StrategyName { get; protected set; } = "Fibonacci";
        public decimal minbet { get; set; }

        public Fibonacci(ILogger logger) : base(logger)
        {

        }

        public Fibonacci()
        {
            
        }

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
        {
            decimal LastBet = PreviousBet.TotalAmount;
            if (Win)
            {
                if (EnableFiboWinIncrement)
                {
                    FibonacciLevel += FiboWinIncrement;
                }
                else if (EnableFiboWinReset)
                {
                    FibonacciLevel = 0;
                }
                else
                {
                    FibonacciLevel = 0;
                    CallStop("Fibonacci bet won.");

                }
            }
            else
            {
                if (EnableFiboLossIncrement)
                {
                    FibonacciLevel += FiboLossIncrement;
                }
                else if (EnableFiboLossReset)
                {
                    FibonacciLevel = 0;
                }
                else
                {
                    FibonacciLevel = 0;
                    CallStop("Fibonacci bet lost.");

                }
            }
            if (FibonacciLevel < 1)
                FibonacciLevel = 1;

            if (FibonacciLevel >= FiboLeve & EnableFiboLevel)
            {
                if (EnableFiboLevelReset)
                    FibonacciLevel = 0;
                else
                {

                    FibonacciLevel = 0;
                    CallStop("Fibonacci level " + FiboLeve + ".");

                }
            }
            LastBet = CalculateFibonacci(FibonacciLevel);
            if (PreviousBet is DiceBet diceb && PreviousBet.Game == Games.Dice)
                return new PlaceDiceBet(LastBet, High, diceb.Chance);
            if (PreviousBet is LimboBet limbob && PreviousBet.Game == Games.Limbo)
                return new PlaceLimboBet(LastBet, limbob.Chance);
            else throw new NotImplementedException("Strategy does not support this game.");
        }

        public override PlaceBet RunReset(Games game)
        {
            FibonacciLevel = 1;
            //return new PlaceDiceBet(CalculateFibonacci(FibonacciLevel),High,(decimal)Chance);
            if (game == Games.Dice)
                return new PlaceDiceBet(CalculateFibonacci(FibonacciLevel), High, Chance);
            if (game == Games.Limbo)
                return new PlaceLimboBet(CalculateFibonacci(FibonacciLevel), 100/ Chance);
            else throw new NotImplementedException("Strategy does not support this game.");
        }
        int Fib(int n)
        {
            return (n < 2) ? n : Fib(n - 1) + Fib(n - 2);
        }
        decimal CalculateFibonacci(int n)
        {
            int x = Fib(n);
            return minbet * (decimal)(x);
        }

        public bool EnableFiboWinIncrement { get; set; } = false;

        public int FibonacciLevel { get; set; } = 1;

        public int FiboWinIncrement { get; set; } = 0;

        public bool EnableFiboWinReset { get; set; } = true;
        public bool EnableFiboWinStop { get; set; } = false;

        public bool EnableFiboLossIncrement { get; set; } = true;

        public int FiboLossIncrement { get; set; } = 1;

        public bool EnableFiboLossReset { get; set; } = false;
        public bool EnableFiboLossStop { get; set; } = false;

        public int FiboLeve { get; set; } = 1;

        public bool EnableFiboLevel { get; set; } = true;

        public bool EnableFiboLevelReset { get; set; } = false;
        public bool EnableFiboLevelStop { get; set; } = false;
        public bool High { get ; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set ; }
        public decimal StartChance { get ; set ; }
    }
}
