using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Twist;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Strategies.Helpers;
using System.ComponentModel;

namespace Gambler.Bot.Strategies.Tests.Helpers
{
    public class InternalBetSettingsTests
    {
        [Fact]
        public void EnableBotSpeed_SetValue_RaisesPropertyChanged()
        {
            var sut = new InternalBetSettings();
            string? propertyName = null;
            sut.PropertyChanged += (_, e) => propertyName = e.PropertyName;

            sut.EnableBotSpeed = true;

            Assert.Equal(nameof(InternalBetSettings.EnableBotSpeed), propertyName);
        }

        [Fact]
        public void BotSpeed_SetValue_RaisesPropertyChanged()
        {
            var sut = new InternalBetSettings();
            string? propertyName = null;
            sut.PropertyChanged += (_, e) => propertyName = e.PropertyName;

            sut.BotSpeed = 2.5m;

            Assert.Equal(nameof(InternalBetSettings.BotSpeed), propertyName);
        }

        [Fact]
        public void RaisePropertyChanged_WithCustomName_RaisesEvent()
        {
            var sut = new InternalBetSettings();
            PropertyChangedEventArgs? args = null;
            sut.PropertyChanged += (_, e) => args = e;

            sut.RaisePropertyChanged("CustomProperty");

            Assert.NotNull(args);
            Assert.Equal("CustomProperty", args!.PropertyName);
        }

        [Fact]
        public void CheckResetPreStats_Always_ReturnsFalse()
        {
            var sut = new InternalBetSettings();

            var result = sut.CheckResetPreStats(new DiceBet(), true, new SessionStats(), new SiteStats());

            Assert.False(result);
        }

        [Fact]
        public void CheckStopPreStats_Always_ReturnsFalseAndEmptyReason()
        {
            var sut = new InternalBetSettings();

            var result = sut.CheckStopPreStats(new DiceBet(), true, new SessionStats(), out var reason, new SiteStats());

            Assert.False(result);
            Assert.Equal(string.Empty, reason);
        }

        [Fact]
        public void CheckResetPostStats_ResetAfterBets_TriggersReset()
        {
            var sut = new InternalBetSettings { EnableResetAfterBets = true, ResetAfterBets = 5 };
            var stats = new SessionStats { Bets = 10 };

            var result = sut.CheckResetPostStats(new DiceBet(), true, stats, new SiteStats());

            Assert.True(result);
        }

        [Fact]
        public void CheckResetPostStats_UpperBalanceLimitReset_TriggersReset()
        {
            var sut = new InternalBetSettings
            {
                EnableUpperLimit = true,
                UpperLimitCompare = "Balance",
                UpperLimitAction = InternalBetSettings.LimitAction.Reset,
                UpperLimit = 10m
            };

            var result = sut.CheckResetPostStats(new DiceBet(), true, new SessionStats(), new SiteStats { Balance = 12m });

            Assert.True(result);
        }

        [Fact]
        public void CheckResetPostStats_UpperProfitLimitReset_ResetsProfitSinceLimitAction()
        {
            var sut = new InternalBetSettings
            {
                EnableUpperLimit = true,
                UpperLimitCompare = "Profit",
                UpperLimitAction = InternalBetSettings.LimitAction.Reset,
                UpperLimit = 3m
            };
            var stats = new SessionStats { PorfitSinceLimitAction = 5m };

            var result = sut.CheckResetPostStats(new DiceBet(), true, stats, new SiteStats());

            Assert.True(result);
            Assert.Equal(0m, stats.PorfitSinceLimitAction);
        }

        [Fact]
        public void CheckResetPostStats_WinBtcStreakReset_ResetsStreakProfit()
        {
            var sut = new InternalBetSettings
            {
                EnableResetAfterBtcStreakWin = true,
                ResetAfterBtcStreakWin = 2m
            };
            var stats = new SessionStats { StreakProfitSinceLastReset = 2.5m };

            var result = sut.CheckResetPostStats(new DiceBet(), true, stats, new SiteStats());

            Assert.True(result);
            Assert.Equal(0m, stats.StreakProfitSinceLastReset);
        }

        [Fact]
        public void CheckResetPostStats_WinBtcReset_ResetsProfitSinceLastReset()
        {
            var sut = new InternalBetSettings
            {
                EnableResetAfterBtcWin = true,
                ResetAfterBtcWin = 1m
            };
            var stats = new SessionStats { ProfitSinceLastReset = 1.2m };

            var result = sut.CheckResetPostStats(new DiceBet(), true, stats, new SiteStats());

            Assert.True(result);
            Assert.Equal(0m, stats.ProfitSinceLastReset);
        }

        [Fact]
        public void CheckResetPostStats_WinFromMinProfit_UpdatesMinProfitSinceReset()
        {
            var sut = new InternalBetSettings
            {
                EnableResetWinFromMinProfit = true,
                ResetWinFromMinProfit = 2m
            };
            var stats = new SessionStats { Profit = 5m, MinProfitSinceReset = 2m };

            var result = sut.CheckResetPostStats(new DiceBet(), true, stats, new SiteStats());

            Assert.True(result);
            Assert.Equal(5m, stats.MinProfitSinceReset);
        }

        [Fact]
        public void CheckResetPostStats_LossBtcStreakReset_ResetsStreakLoss()
        {
            var sut = new InternalBetSettings
            {
                EnableResetAfterBtcStreakLoss = true,
                ResetAfterBtcStreakLoss = 2m
            };
            var stats = new SessionStats { StreakLossSinceLastReset = -3m };

            var result = sut.CheckResetPostStats(new DiceBet(), false, stats, new SiteStats());

            Assert.True(result);
            Assert.Equal(0m, stats.StreakLossSinceLastReset);
        }

        [Fact]
        public void CheckResetPostStats_LossBtcReset_ResetsProfitSinceLastReset()
        {
            var sut = new InternalBetSettings
            {
                EnableResetAfterBtcLoss = true,
                ResetAfterBtcLoss = 2m
            };
            var stats = new SessionStats { ProfitSinceLastReset = -2.1m };

            var result = sut.CheckResetPostStats(new DiceBet(), false, stats, new SiteStats());

            Assert.True(result);
            Assert.Equal(0m, stats.ProfitSinceLastReset);
        }

        [Fact]
        public void CheckResetPostStats_LossFromMaxProfit_UpdatesMaxProfitSinceReset()
        {
            var sut = new InternalBetSettings
            {
                EnableResetLossFromMaxProfit = true,
                ResetLossFromMaxProfit = 3m
            };
            var stats = new SessionStats { MaxProfitSinceReset = 10m, Profit = 6m };

            var result = sut.CheckResetPostStats(new DiceBet(), false, stats, new SiteStats());

            Assert.True(result);
            Assert.Equal(6m, stats.MaxProfitSinceReset);
        }

        [Fact]
        public void CheckResetPostStats_NoConditionsMet_ReturnsFalse()
        {
            var sut = new InternalBetSettings();

            var result = sut.CheckResetPostStats(new DiceBet(), true, new SessionStats(), new SiteStats());

            Assert.False(result);
        }

        [Fact]
        public void CheckStopPostStats_UpperBalanceStop_ReturnsTrueAndReason()
        {
            var sut = new InternalBetSettings
            {
                EnableUpperLimit = true,
                UpperLimitCompare = "Balance",
                UpperLimitAction = InternalBetSettings.LimitAction.Stop,
                UpperLimit = 10m
            };

            var result = sut.CheckStopPOstStats(new DiceBet(), true, new SessionStats(), out var reason, new SiteStats { Balance = 11m });

            Assert.True(result);
            Assert.Equal("Upper balance limit reached.", reason);
        }

        [Fact]
        public void CheckStopPostStats_StopAfterBets_ReturnsTrueAndReason()
        {
            var sut = new InternalBetSettings
            {
                EnableStopAfterBets = true,
                StopAfterBets = 5
            };
            var stats = new SessionStats { Bets = 5 };

            var result = sut.CheckStopPOstStats(new DiceBet(), true, stats, out var reason, new SiteStats());

            Assert.True(result);
            Assert.Contains("Stop after 5 bets", reason);
        }

        [Fact]
        public void CheckStopPostStats_WinStreakStop_ReturnsTrue()
        {
            var sut = new InternalBetSettings
            {
                EnableStopAfterWinStreak = true,
                StopAfterWinStreak = 3
            };
            var stats = new SessionStats { WinStreak = 3 };

            var result = sut.CheckStopPOstStats(new DiceBet(), true, stats, out var reason, new SiteStats());

            Assert.True(result);
            Assert.Contains("Wines in a row", reason);
        }

        [Fact]
        public void CheckStopPostStats_BtcLossStop_ReturnsTrue()
        {
            var sut = new InternalBetSettings
            {
                EnableStopAfterBtcLoss = true,
                StopAfterBtcLoss = 2m
            };
            var stats = new SessionStats { Profit = -2.5m };

            var result = sut.CheckStopPOstStats(new DiceBet(), false, stats, out var reason, new SiteStats());

            Assert.True(result);
            Assert.Contains("Currency Loss", reason);
        }

        [Fact]
        public void CheckStopPostStats_NoConditionsMet_ReturnsFalseAndEmptyReason()
        {
            var sut = new InternalBetSettings();

            var result = sut.CheckStopPOstStats(new DiceBet(), true, new SessionStats(), out var reason, new SiteStats());

            Assert.False(result);
            Assert.Equal(string.Empty, reason);
        }

        [Fact]
        public void CheckWithdraw_UpperBalanceLimit_TriggersWithdrawWithAmountAndAddress()
        {
            var sut = new InternalBetSettings
            {
                EnableUpperLimit = true,
                UpperLimitCompare = "Balance",
                UpperLimitAction = InternalBetSettings.LimitAction.Withdraw,
                UpperLimit = 10m,
                UpperLimitActionAmount = 3m,
                UpperLimitAddress = "wallet123"
            };

            var result = sut.CheckWithdraw(new DiceBet(), true, new SessionStats(), out var amount, new SiteStats { Balance = 20m }, out var address);

            Assert.True(result);
            Assert.Equal(3m, amount);
            Assert.Equal("wallet123", address);
        }

        [Fact]
        public void CheckTips_UpperProfitLimit_TriggersTipWithAmountAndAddress()
        {
            var sut = new InternalBetSettings
            {
                EnableUpperLimit = true,
                UpperLimitCompare = "Profit",
                UpperLimitAction = InternalBetSettings.LimitAction.Tip,
                UpperLimit = 4m,
                UpperLimitActionAmount = 1.5m,
                UpperLimitAddress = "tip-user"
            };
            var stats = new SessionStats { Profit = 5m };

            var result = sut.CheckTips(new DiceBet(), true, stats, out var amount, new SiteStats(), out var address);

            Assert.True(result);
            Assert.Equal(1.5m, amount);
            Assert.Equal("tip-user", address);
        }

        [Fact]
        public void CheckBank_LowerBalanceLimit_TriggersBankAmount()
        {
            var sut = new InternalBetSettings
            {
                EnableLowerLimit = true,
                UpperLimitCompare = "Balance",
                LowerLimitAction = InternalBetSettings.LimitAction.Bank,
                LowerLimit = 2m,
                LowerLimitActionAmount = 0.25m
            };

            var result = sut.CheckBank(new DiceBet(), false, new SessionStats(), out var amount, new SiteStats { Balance = 1m });

            Assert.True(result);
            Assert.Equal(0.25m, amount);
        }

        [Fact]
        public void CheckWithdraw_NoConditionsMet_ReturnsFalseWithDefaultOutputs()
        {
            var sut = new InternalBetSettings();

            var result = sut.CheckWithdraw(new DiceBet(), true, new SessionStats(), out var amount, new SiteStats { Balance = 1m }, out var address);

            Assert.False(result);
            Assert.Equal(0m, amount);
            Assert.Null(address);
        }

        [Fact]
        public void CheckHighLow_UnsupportedBetType_ReturnsFalse()
        {
            var sut = new InternalBetSettings();

            var result = sut.CheckHighLow(new UnknownBet(), true, new SessionStats(), out var newHigh, new SiteStats());

            Assert.False(result);
            Assert.False(newHigh);
        }

        [Theory]
        [InlineData(true, 4, true)]
        [InlineData(true, 3, false)]
        [InlineData(false, 4, false)]
        public void CheckHighLow_SwitchWins_Permutations(bool enabled, long wins, bool expectedTriggered)
        {
            var sut = new InternalBetSettings
            {
                EnableSwitchWins = enabled,
                SwitchWins = 2
            };
            var stats = new SessionStats { Wins = wins };

            var result = sut.CheckHighLow(new DiceBet { High = true }, true, stats, out var newHigh, new SiteStats());

            Assert.Equal(expectedTriggered, result);
            Assert.Equal(expectedTriggered ? false : false, newHigh);
        }

        [Theory]
        [InlineData(true, 6, true)]
        [InlineData(true, 5, false)]
        [InlineData(false, 6, false)]
        public void CheckHighLow_SwitchWinStreak_Permutations(bool enabled, long winStreak, bool expectedTriggered)
        {
            var sut = new InternalBetSettings
            {
                EnableSwitchWinStreak = enabled,
                SwitchWinStreak = 3
            };
            var stats = new SessionStats { WinStreak = winStreak };

            var result = sut.CheckHighLow(new DiceBet { High = true }, true, stats, out var newHigh, new SiteStats());

            Assert.Equal(expectedTriggered, result);
            Assert.Equal(expectedTriggered ? false : false, newHigh);
        }

        [Theory]
        [InlineData(true, 6, true)]
        [InlineData(true, 5, false)]
        [InlineData(false, 6, false)]
        public void CheckHighLow_SwitchLosses_Permutations(bool enabled, long losses, bool expectedTriggered)
        {
            var sut = new InternalBetSettings
            {
                EnableSwitchLosses = enabled,
                SwitchLosses = 3
            };
            var stats = new SessionStats { Losses = losses };

            var result = sut.CheckHighLow(new TwistBet { High = false }, false, stats, out var newHigh, new SiteStats());

            Assert.Equal(expectedTriggered, result);
            Assert.Equal(expectedTriggered ? true : false, newHigh);
        }

        [Theory]
        [InlineData(true, 8, true)]
        [InlineData(true, 7, false)]
        [InlineData(false, 8, false)]
        public void CheckHighLow_SwitchLossStreak_Permutations(bool enabled, long lossStreak, bool expectedTriggered)
        {
            var sut = new InternalBetSettings
            {
                EnableSwitchLossStreak = enabled,
                SwitchLossStreak = 4
            };
            var stats = new SessionStats { LossStreak = lossStreak };

            var result = sut.CheckHighLow(new DiceBet { High = true }, false, stats, out var newHigh, new SiteStats());

            Assert.Equal(expectedTriggered, result);
            Assert.Equal(expectedTriggered ? false : false, newHigh);
        }

        [Theory]
        [InlineData(true, 10, true)]
        [InlineData(true, 9, false)]
        [InlineData(false, 10, false)]
        public void CheckHighLow_SwitchBets_Permutations(bool enabled, long bets, bool expectedTriggered)
        {
            var sut = new InternalBetSettings
            {
                EnableSwitchBets = enabled,
                SwitchBets = 5
            };
            var stats = new SessionStats { Bets = bets };

            var result = sut.CheckHighLow(new DiceBet { High = true }, false, stats, out var newHigh, new SiteStats());

            Assert.Equal(expectedTriggered, result);
            Assert.Equal(expectedTriggered ? false : false, newHigh);
        }

        [Fact]
        public void CheckResetSeed_ResetOnBets_ReturnsTrue()
        {
            var sut = new InternalBetSettings
            {
                EnableResetSeedBets = true,
                ResetSeedBets = 5
            };
            var stats = new SessionStats { Bets = 10 };

            var result = sut.CheckResetSeed(new DiceBet(), true, stats, new SiteStats());

            Assert.True(result);
        }

        [Fact]
        public void CheckResetSeed_ResetOnWinStreakRequiresWin()
        {
            var sut = new InternalBetSettings
            {
                EnableResetSeedWinStreak = true,
                ResetSeedWinStreak = 3
            };
            var stats = new SessionStats { WinStreak = 3 };

            var result = sut.CheckResetSeed(new DiceBet(), true, stats, new SiteStats());

            Assert.True(result);
        }

        [Fact]
        public void CheckResetSeed_ResetOnLossStreakRequiresLoss()
        {
            var sut = new InternalBetSettings
            {
                EnableResetSeedLossStreak = true,
                ResetSeedLossStreak = 4
            };
            var stats = new SessionStats { LossStreak = 4 };

            var result = sut.CheckResetSeed(new DiceBet(), false, stats, new SiteStats());

            Assert.True(result);
        }

        [Fact]
        public void CheckResetSeed_NoConditionsMet_ReturnsFalse()
        {
            var sut = new InternalBetSettings();

            var result = sut.CheckResetSeed(new DiceBet(), true, new SessionStats(), new SiteStats());

            Assert.False(result);
        }

        private sealed class UnknownBet : Bet
        {
            public override PlaceBet CreateRetry() => throw new NotImplementedException();
            public override bool GetWin(IGameConfig config) => false;
            public override string ToCSV(IGameConfig gamecofig, long TotalBetsPlaced, decimal Balance) => string.Empty;
        }
    }
}