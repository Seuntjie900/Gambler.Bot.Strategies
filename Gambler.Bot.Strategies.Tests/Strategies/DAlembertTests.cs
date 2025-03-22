using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Strategies.Strategies;
using Gambler.Bot.Common.Games;
using Microsoft.Extensions.Logging;
using Moq;
using Gambler.Bot.Common.Games.Dice;


namespace Gambler.Bot.Strategies.Tests.Strategies
{
    public class DAlembertTests
    {

        //DAlembert tests:
        /*
        win:
            StretchWin = 1 && winstreak = 1, 2, 3 should increment bet
            Different incrementWin values
            negative incrementwin values
            different stretchWin values with different winstreaks
            Bet below minbet should be minbet

        Lose:
            StretchLose = 1 && Losestreak = 1, 2, 3 should increment bet
            Different incrementLose values
            negative incrementLose values
            different stretchLose values with different Losestreaks
            Bet below minbet should be minbet


        RunReset:
            Should reset the strategy
         */

        private readonly DAlembert _strategy;
        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();

        public DAlembertTests()
        {
            _strategy = new DAlembert(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_Sets_DefaultValues()
        {
            Assert.Equal("D'Alembert", _strategy.StrategyName);
            Assert.Equal(0, _strategy.AlembertStretchWin);
            Assert.Equal(0, _strategy.AlembertStretchLoss);
            Assert.Equal(0.00000100m, _strategy.AlembertIncrementLoss);
            Assert.Equal(0.00000100m, _strategy.MinBet);
            Assert.Equal(0.00000100m, _strategy.AlembertIncrementWin);
        }


        [Theory]
        [InlineData(1, 0.00000200)]
        [InlineData(2, 0.00000200)]
        [InlineData(3, 0.00000200)]
        public void CalculateNextDiceBet_Win_IncrementsBetOnStretchWin(int winStreak, decimal expectedAmount)
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000100m, Chance = 0.1m };
            _strategy.AlembertStretchWin = 0;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { WinStreak = winStreak };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, true);

            // Assert
            Assert.Equal(expectedAmount, result.Amount);
        }

        [Theory]
        [InlineData(0.00000100, 0.00000200)]
        [InlineData(0.00001000, 0.00001100)]
        public void CalculateNextDiceBet_Win_DifferentIncrementWinValues(decimal increment, decimal expectedAmount)
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000100m, Chance = 0.1m };
            _strategy.AlembertIncrementWin = increment;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { WinStreak = 1 };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, true);

            // Assert
            Assert.Equal(expectedAmount, result.Amount);
        }

        [Theory]
        [InlineData(-0.00000050, 0.00000450)]
        public void CalculateNextDiceBet_Win_NegativeIncrementWin(decimal increment, decimal expectedAmount)
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000500m, Chance = 0.1m };
            _strategy.AlembertIncrementWin = increment;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { WinStreak = 1 };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, true);

            // Assert
            Assert.Equal(expectedAmount, result.Amount);
        }

        [Theory]
        [InlineData(1, 1, 0.00000100)]  // No increment due to win streak not matching stretch
        [InlineData(1, 2, 0.00000200)]  // Increment occurs as win streak matches stretch
        [InlineData(2, 4, 0.00000100)]  // No increment due to win streak not matching stretch
        [InlineData(2, 3, 0.00000200)]  // Increment occurs as win streak matches stretch
        [InlineData(2, 6, 0.00000200)]  // Increment occurs as win streak matches stretch
        [InlineData(6, 25, 0.00000100)]  // No increment due to win streak not matching stretch
        [InlineData(6, 21, 0.00000200)]  // Increment occurs as win streak matches stretch
        public void CalculateNextDiceBet_Win_DifferentStretchWinValues(int stretchWin, int winStreak, decimal expectedAmount)
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000100m, Chance = 0.1m };
            _strategy.AlembertStretchWin = stretchWin;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { WinStreak = winStreak };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, true);

            // Assert
            Assert.Equal(expectedAmount, result.Amount);
        }

        [Fact]
        public void CalculateNextDiceBet_Win_BetBelowMinBet_ShouldBeMinBet()
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000050m, Chance = 0.1m };
            _strategy.MinBet = 0.0000100m;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { WinStreak = 1 };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, true);

            // Assert
            Assert.Equal(_strategy.MinBet, result.Amount);
        }



        [Theory]
        [InlineData(1, 0.00000200)]
        [InlineData(2, 0.00000200)]
        [InlineData(3, 0.00000200)]
        public void CalculateNextDiceBet_Lose_IncrementsBetOnStretchLose(int LoseStreak, decimal expectedAmount)
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000100m, Chance = 0.1m };
            _strategy.AlembertStretchLoss = 0;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { LossStreak = LoseStreak };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, false);

            // Assert
            Assert.Equal(expectedAmount, result.Amount);
        }

        [Theory]
        [InlineData(0.00000100, 0.00000200)]
        [InlineData(0.00001000, 0.00001100)]
        public void CalculateNextDiceBet_Lose_DifferentIncrementLoseValues(decimal increment, decimal expectedAmount)
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000100m, Chance = 0.1m };
            _strategy.AlembertIncrementLoss = increment;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { LossStreak = 1 };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, false);

            // Assert
            Assert.Equal(expectedAmount, result.Amount);
        }

        [Theory]
        [InlineData(-0.00000050, 0.00000450)]
        public void CalculateNextDiceBet_Lose_NegativeIncrementLose(decimal increment, decimal expectedAmount)
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000500m, Chance = 0.1m };
            _strategy.AlembertIncrementLoss = increment;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { LossStreak = 1 };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, false);

            // Assert
            Assert.Equal(expectedAmount, result.Amount);
        }

        [Theory]
        [InlineData(1, 1, 0.00000100)]  // No increment due to Lose streak not matching stretch
        [InlineData(1, 2, 0.00000200)]  // Increment occurs as Lose streak matches stretch
        [InlineData(2, 4, 0.00000100)]  // No increment due to Lose streak not matching stretch
        [InlineData(2, 3, 0.00000200)]  // Increment occurs as Lose streak matches stretch
        [InlineData(2, 6, 0.00000200)]  // Increment occurs as Lose streak matches stretch
        [InlineData(6, 25, 0.00000100)]  // No increment due to Lose streak not matching stretch
        [InlineData(6, 21, 0.00000200)]  // Increment occurs as Lose streak matches stretch
        public void CalculateNextDiceBet_Lose_DifferentStretchLoseValues(int stretchLose, int LoseStreak, decimal expectedAmount)
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000100m, Chance = 0.1m };
            _strategy.AlembertStretchLoss = stretchLose;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { LossStreak = LoseStreak };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, false);

            // Assert
            Assert.Equal(expectedAmount, result.Amount);
        }

        [Fact]
        public void CalculateNextDiceBet_Lose_BetBelowMinBet_ShouldBeMinBet()
        {
            // Arrange
            var previousBet = new DiceBet { TotalAmount = 0.00000050m, Chance = 0.1m };
            _strategy.MinBet = 0.0000100m;
            _strategy.OnNeedStats += ((e, args) =>
            {
                return new SessionStats { LossStreak = 1 };
            });

            // Act
            var result = _strategy.CalculateNextBet(previousBet, false);

            // Assert
            Assert.Equal(_strategy.MinBet, result.Amount);
        }

        // You can mirror the above tests for the Lose scenarios by adjusting the conditions and using LossStreak instead of WinStreak.

        [Fact]
        public void RunReset_ShouldResetStrategy()
        {
            // Arrange
            _strategy.MinBet = 0.00000100m;
            _strategy.Chance = 0.5m;
            _strategy.High = true;  // Initial condition to check reset

            // Act
            var result = _strategy.RunReset(Games.Dice);

            // Assert
            Assert.Equal(_strategy.MinBet, (result as PlaceDiceBet).Amount);
            Assert.Equal(_strategy.Chance, (result as PlaceDiceBet).Chance);
        }
    }
}
