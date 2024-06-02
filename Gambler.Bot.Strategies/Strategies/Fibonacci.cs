using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.Common.Games;
using Microsoft.Extensions.Logging;
using System;

namespace Gambler.Bot.Strategies.Strategies
{
    public class Fibonacci: BaseStrategy, iDiceStrategy
    {
        public override string StrategyName { get; protected set; } = "Fibonacci";
        public decimal minbet { get; set; }

        public Fibonacci(ILogger logger) : base(logger)
        {

        }

        public Fibonacci()
        {
            
        }

        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
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
            if (FibonacciLevel < 0)
                FibonacciLevel = 0;

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
            return new PlaceDiceBet(LastBet, High, PreviousBet.Chance);
        }

        public override PlaceDiceBet RunReset()
        {
            FibonacciLevel = 1;
            return new PlaceDiceBet(CalculateFibonacci(FibonacciLevel),High,(decimal)Chance);
        }

        decimal CalculateFibonacci(int n)
        {
            int x = (int)((1.0 / (Math.Sqrt(5.0))) * (Math.Pow((1.0 + Math.Sqrt(5.0)) / 2.0, n) - Math.Pow((1.0 + Math.Sqrt(5.0)) / 2.0, n)));
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
