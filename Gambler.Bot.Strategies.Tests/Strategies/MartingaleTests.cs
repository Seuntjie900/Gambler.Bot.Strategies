using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.Common.Games.Twist;
using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Strategies.Strategies;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gambler.Bot.Strategies.Tests.Strategies
{
    public class MartingaleTests
    {
        [Fact]
        public void Constructor_Sets_DefaultValues()
        {
            var strategy = CreateStrategy(new SessionStats());

            Assert.Equal("Martingale", strategy.StrategyName);
            Assert.Equal(0.00000100m, strategy.MinBet);
            Assert.Equal(2m, strategy.Multiplier);
            Assert.Equal(2m, strategy.BaseMultiplier);
            Assert.Equal(49.5m, strategy.BaseChance);
        }

        [Fact]
        public void CalculateNextBet_Win_VariableMode_UpdatesWinMultiplierAndAmount()
        {
            var strategy = CreateStrategy(new SessionStats { WinStreak = 2 });
            strategy.WinMultiplierMode = MartingaleMultiplierMode.Variable;
            strategy.WinDevidecounter = 2;
            strategy.WinDevider = 3m;
            strategy.WinBaseMultiplier = 2m;
            strategy.RunReset(Games.Dice);

            var previousBet = new DiceBet { TotalAmount = 1m, Chance = 49.5m, High = true };

            var result = strategy.CalculateNextBet(previousBet, true);

            Assert.Equal(6m, strategy.WinMultiplier);
            Assert.Equal(6m, result.Amount);
        }

        [Fact]
        public void CalculateNextBet_Win_FirstWin_ResetsToMinBetAndBaseChance()
        {
            var strategy = CreateStrategy(new SessionStats { WinStreak = 1 });
            strategy.EnableFirstResetWin = true;
            strategy.EnableMK = false;
            strategy.BaseChance = 33.3m;
            strategy.MinBet = 0.00000200m;

            var previousBet = new DiceBet { TotalAmount = 1m, Chance = 49.5m, High = true };

            var result = strategy.CalculateNextBet(previousBet, true);

            var diceResult = Assert.IsType<PlaceDiceBet>(result);
            Assert.Equal(strategy.MinBet, diceResult.Amount);
            Assert.Equal(33.3m, diceResult.Chance);
        }

        [Fact]
        public void CalculateNextBet_Win_EnableTrazel_UsesTrazelWinAmountAndFlipsHigh()
        {
            var strategy = CreateStrategy(new SessionStats { WinStreak = 3 });
            strategy.EnableTrazel = true;
            strategy.starthigh = true;
            strategy.TrazelWin = 2;
            strategy.trazelwin = 2;
            strategy.trazelwinto = 7m;

            var previousBet = new DiceBet { TotalAmount = 1m, Chance = 40m, High = true };

            var result = strategy.CalculateNextBet(previousBet, true);

            var diceResult = Assert.IsType<PlaceDiceBet>(result);
            Assert.Equal(7m, diceResult.Amount);
            Assert.False(diceResult.High);
            Assert.True(strategy.trazelmultiply);
        }

        [Fact]
        public void CalculateNextBet_Loss_ChangeOnce_DoesNotDropMultiplierBelowOne()
        {
            var strategy = CreateStrategy(new SessionStats { LossStreak = 2 });
            strategy.MultiplierMode = MartingaleMultiplierMode.ChangeOnce;
            strategy.Devidecounter = 2;
            strategy.Devider = 0.1m;
            strategy.BaseMultiplier = 2m;
            strategy.RunReset(Games.Dice);

            var previousBet = new DiceBet { TotalAmount = 1m, Chance = 49.5m, High = true };

            var result = strategy.CalculateNextBet(previousBet, false);

            Assert.Equal(1m, strategy.Multiplier);
            Assert.Equal(1m, result.Amount);
        }

        [Fact]
        public void CalculateNextBet_Loss_EnableTrazelLose_SetsConfiguredAmountAndFlipsHigh()
        {
            var strategy = CreateStrategy(new SessionStats { LossStreak = 2 });
            strategy.EnableTrazel = true;
            strategy.TrazelLose = 3;
            strategy.trazelmultiply = false;
            strategy.trazelloseto = 9m;
            strategy.starthigh = true;

            var previousBet = new DiceBet { TotalAmount = 1m, Chance = 49.5m, High = true };

            var result = strategy.CalculateNextBet(previousBet, false);

            var diceResult = Assert.IsType<PlaceDiceBet>(result);
            Assert.Equal(18m, diceResult.Amount);
            Assert.False(diceResult.High);
            Assert.True(strategy.trazelmultiply);
        }

        [Fact]
        public void CalculateNextBet_UsesPercentageOfBalance_WhenEnabled()
        {
            var strategy = CreateStrategy(new SessionStats { LossStreak = 1 }, balance: 250m);
            strategy.EnablePercentage = true;
            strategy.Percentage = 2m;

            var previousBet = new DiceBet { TotalAmount = 10m, Chance = 49.5m, High = true };

            var result = strategy.CalculateNextBet(previousBet, false);

            Assert.Equal(5m, result.Amount);
        }

        [Fact]
        public void CalculateNextBet_Limbo_CreatesPlaceLimboBet_WithExpectedPayout()
        {
            var strategy = CreateStrategy(new SessionStats { WinStreak = 1 }, edge: 1m);
            strategy.Chance = 49.5m;

            var previousBet = new LimboBet { TotalAmount = 2m, Payout = 2m };

            var result = strategy.CalculateNextBet(previousBet, true);

            var limboResult = Assert.IsType<PlaceLimboBet>(result);
            Assert.Equal(0.00000100m, limboResult.Amount);
            Assert.Equal((100m - 1m) / strategy.Chance, limboResult.Payout);
        }

        [Fact]
        public void CalculateNextBet_Twist_UsesPreviousTwistChance()
        {
            var strategy = CreateStrategy(new SessionStats { WinStreak = 1 });
            strategy.High = true;

            var previousBet = new TwistBet { TotalAmount = 3m, Chance = 12.34m, High = false };

            var result = strategy.CalculateNextBet(previousBet, true);

            var twistResult = Assert.IsType<PlaceTwistBet>(result);
            Assert.Equal(12.34m, twistResult.Chance);
        }

        [Fact]
        public void CalculateNextBet_Crash_CreatesPlaceCrashBet_WithCurrentMapping()
        {
            var strategy = CreateStrategy(new SessionStats { LossStreak = 1 });
            var previousBet = new CrashBet { TotalAmount = 4m, Payout = 1.5m };

            var result = strategy.CalculateNextBet(previousBet, false);

            var crashResult = Assert.IsType<PlaceCrashBet>(result);
            Assert.Equal(8m, crashResult.Payout);
            Assert.Equal(1.5m, crashResult.Amount);
        }

        [Fact]
        public void CalculateNextBet_WhenGameIsUnsupported_ThrowsNotImplementedException()
        {
            var strategy = CreateStrategy(new SessionStats { WinStreak = 1 });
            var previousBet = new UnknownBet { TotalAmount = 1m, Game = (Games)999 };

            Assert.Throws<NotImplementedException>(() => strategy.CalculateNextBet(previousBet, true));
        }

        [Fact]
        public void RunReset_ResetsStateAndReturnsDiceBet()
        {
            var strategy = CreateStrategy(new SessionStats());
            strategy.MinBet = 0.00000300m;
            strategy.starthigh = false;
            strategy.BaseChance = 21m;
            strategy.BaseMultiplier = 4m;
            strategy.WinBaseMultiplier = 5m;

            var result = strategy.RunReset(Games.Dice);

            var diceResult = Assert.IsType<PlaceDiceBet>(result);
            Assert.Equal(strategy.MinBet, diceResult.Amount);
            Assert.False(diceResult.High);
            Assert.Equal(21m, diceResult.Chance);
            Assert.Equal(4m, strategy.Multiplier);
            Assert.Equal(5m, strategy.WinMultiplier);
        }

        private static Martingale CreateStrategy(SessionStats stats, decimal balance = 0m, decimal edge = 1m)
        {
            var logger = new Mock<ILogger>();
            var strategy = new Martingale(logger.Object)
            {
                Config = new DiceConfig { Edge = edge, MaxRoll = 100m }
            };

            strategy.OnNeedStats += (_, _) => stats;
            strategy.NeedBalance += () => balance;

            return strategy;
        }

        private sealed class UnknownBet : Bet
        {
            public override PlaceBet CreateRetry() => throw new NotImplementedException();
            public override bool GetWin(IGameConfig config) => false;
            public override string ToCSV(IGameConfig gamecofig, long TotalBetsPlaced, decimal Balance) => string.Empty;
        }
    }
}
