using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatBot.Helpers
{
    public class InternalBetSettings
    {

        /*
         * reset seed settings
         * reset settings
         * withdraw/bank/tip amount and conditions
         * stop settings
         * bank settings
         * high/low settings
         * 
         */
        public bool EnableStopAfterBets { get; set; } = false;
        public int StopAfterBets { get; set; }
        public bool EnableResetAfterBets { get; set; }
        public int ResetAfterBets { get; set; }
        public bool EnableStopAfterTime { get; set; }
        public long StopAfterTime { get; set; }
        public bool EnableResetAfterLoseStreak { get; set; }
        public int ResetAfterLoseStreak { get; set; }
        public bool EnableStopAfterLoseStreak { get; set; }
        public int StopAfterLoseStreak { get; set; }
        public bool EnableStopAfterBtcLoseStreak { get; set; }
        public decimal StopAfterBtcLoseStreak { get; set; }
        public bool EnableStopAfterBtcLoss { get; set; }
        public decimal StopAfterBtcLoss { get; set; }
        public bool EnableResetAfterBtcStreakLoss { get; set; }
        public decimal ResetAfterBtcStreakLoss { get; set; }
        public bool EnableResetAfterBtcLoss { get; set; }
        public decimal ResetAfterBtcLoss { get; set; }
        public bool EnableStopAfterLosses { get; set; }
        public int StopAfterLosses { get; set; }
        public bool EnableResetAfterLosses { get; set; }
        public int ResetAfterLosses { get; set; }

        public bool EnableResetAfterWinStreak { get; set; } = false;
        public int ResetAfterWinStreak { get; set; }
        public bool EnableStopAfterWinStreak { get; set; } = false;
        public int StopAfterWinStreak { get; set; }
        public bool EnableStopAfterBtcWinStreak { get; set; } = false;
        public decimal StopAfterBtcWinStreak { get; set; }
        public bool EnableStopAfterBtcWin { get; set; } = false;
        public decimal StopAfterBtcWin { get; set; }
        public bool EnableResetAfterBtcStreakWin { get; set; } = false;
        public decimal ResetAfterBtcStreakWin { get; set; }
        public bool EnableResetAfterBtcWin { get; set; } = false;
        public decimal ResetAfterBtcWin { get; set; }
        public bool EnableStopAfterWins { get; set; } = false;
        public int StopAfterWins { get; set; }
        public bool EnableResetAfterWins { get; set; } = false;
        public int ResetAfterWins { get; set; }

        public bool EnableUpperBalanceLimit { get; set; } = false;
        public decimal UpperBalanceLimit { get; set; }
        public bool EnableLowerBalanceLimit { get; set; } = false;
        public decimal LowerBalanceLimit { get; set; }
        public enum LimitAction { Stop, Withdraw, Tip, Reset, Invest, Bank }
        public LimitAction UpperBalanceLimitAction { get; set; }
        public LimitAction LowerBalanceLimitAction { get; set; }
        public decimal UpperBalanceLimitActionAmount { get; set; }
        public decimal LowerBalanceLimitActionAmount { get; set; }

        public bool EnableUpperProfitLimitt { get; set; } = false;

        public decimal UpperProfitLimit { get; set; }
        public bool EnableLowerProfitLimit { get; set; } = false;

        public decimal LowerProfitLimit { get; set; }
        public LimitAction UpperProfitLimitAction { get; set; }
        public LimitAction LowerProfitLimitAction { get; set; }
        public decimal UpperProfitLimitActionAmount { get; set; }
        public decimal LowerProfitLimitActionAmount { get; set; }

        public bool EnableMaxBet { get; set; } = false;
        public bool EnableMinBet { get; set; } = false;
        public decimal MaxBet { get; set; }
        public decimal MinBet { get; set; }

        public bool EnableBotSpeed { get; set; } = false;
        public decimal BotSpeed { get; set; }

        public bool EnableResetSeedBets { get; set; } = false;
        public int ResetSeedBets { get; set; }
        public bool EnableResetSeedWins { get; set; } = false;
        public int ResetSeedWins { get; set; }
        public bool EnableResetSeedLosses { get; set; } = false;
        public int ResetSeedLosses { get; set; }
        public bool EnableResetSeedWinStreak { get; set; } = false;
        public int ResetSeedWinStreak { get; set; }
        public bool EnableResetSeedLossStreak { get; set; } = false;
        public int ResetSeedLossStreak { get; set; }


        public bool EnableSwitchWins { get; set; } = false;
        public int SwitchWins { get; set; }
        public bool EnableSwitchWinStreak { get; set; } = false;
        public int SwitchWinStreak { get; set; }
        public bool EnableSwitchLosses { get; set; } = false;
        public int SwitchLosses { get; set; }
        public bool EnableSwitchLossStreak { get; set; } = false;
        public int SwitchLossStreak { get; set; }
        public bool EnableSwitchBets { get; set; } = false;
        public int SwitchBets { get; set; }

        public Trigger[] Triggers { get; private set; } = new Trigger[0];

        #region Process Bet


        public bool CheckResetPreStats(DiceBet NewBet, bool win, SessionStats Stats)
        {
            return false;
        }
        public bool CheckResetPostStats(DiceBet NewBet, bool win, SessionStats Statsn)
        {
            bool reset = false;

            if (EnableResetAfterBets && Statsn.Bets % ResetAfterBets==0)
            {
                reset = true;
            }
            if (EnableUpperBalanceLimit && UpperBalanceLimitAction == LimitAction.Reset)
            {
                //if balance is larger than the limit, reset
            }
            if (EnableLowerBalanceLimit && LowerBalanceLimitAction == LimitAction.Reset)
            {
                //if balance is Smaller than the limit, reset
            }
            if (EnableUpperProfitLimitt && UpperProfitLimitAction == LimitAction.Reset)
            {
                if (Statsn.PorfitSinceLimitAction>= UpperProfitLimit)
                {
                    reset = true;
                    Statsn.PorfitSinceLimitAction = 0;
                }
            }
            if (EnableLowerProfitLimit && LowerProfitLimitAction == LimitAction.Reset)
            {
                if (Statsn.PorfitSinceLimitAction >= LowerProfitLimit)
                {
                    reset = true;
                    Statsn.PorfitSinceLimitAction = 0;
                }
            }
            if (win)
            {
                if (EnableResetAfterWinStreak && Statsn.WinStreak % ResetAfterWinStreak == 0)
                {
                    reset = true;
                }
                if (EnableResetAfterBtcStreakWin && Statsn.StreakProfitSinceLastReset <= ResetAfterBtcStreakWin)
                {
                    reset = true;
                    Statsn.StreakProfitSinceLastReset = 0;
                }
                if (EnableResetAfterBtcWin && Statsn.ProfitSinceLastReset <= ResetAfterBtcWin)
                {
                    reset = true;
                    Statsn.ProfitSinceLastReset = 0;
                }
                if (EnableResetAfterWins && Statsn.Wins % ResetAfterWins == 0)
                {
                    reset = true;
                }
            }
            else
            {
                if (EnableResetAfterLoseStreak && Statsn.LossStreak % ResetAfterLoseStreak==0 )
                {
                    reset = true;
                }
                if (EnableResetAfterBtcStreakLoss && Statsn.StreakProfitSinceLastReset <= ResetAfterBtcStreakLoss)
                {
                    reset = true;
                    Statsn.StreakProfitSinceLastReset=0;
                }
                if (EnableResetAfterBtcLoss && Statsn.ProfitSinceLastReset<= ResetAfterBtcLoss)
                {
                    reset = true;
                    Statsn.ProfitSinceLastReset = 0;
                }
                if (EnableResetAfterLosses && Statsn.Losses%ResetAfterLosses==0)
                {
                    reset = true;
                }
            }
            return reset;
        }

        public bool CheckStopPreStats(DiceBet NewBet, bool win, SessionStats Stats, out string reason)
        {
            reason = "";
            return false;
        }

        public bool CheckStopPOstStats(DiceBet NewBet, bool win, SessionStats Stats, out string reason)
        {
            
            if (EnableUpperBalanceLimit && UpperBalanceLimitAction == LimitAction.Stop)
            {
                //check balance
                //stop if balance is larger
            }
            if (EnableLowerBalanceLimit && LowerBalanceLimitAction == LimitAction.Stop)
            {
                //check balance
                //stop if balance is lower
            }
            if (EnableUpperProfitLimitt && UpperProfitLimitAction == LimitAction.Stop)
            {
                if (Stats.Profit>=UpperProfitLimit)
                {
                    reason = "Upper profit limit reached. Stopping.";
                    return true;
                }
            }
            if (EnableLowerProfitLimit && LowerProfitLimitAction == LimitAction.Stop)
            {
                if (Stats.Profit <= LowerProfitLimit)
                {
                    reason = "Lower profit limit reached. Stopping.";
                    return true;
                }
            }
            if (EnableStopAfterBets && Stats.Bets>= StopAfterBets)
            {
                reason = "Stop after "+StopAfterBets+" bets condition triggered with "+Stats.Bets+" bets, Stopping.";
                return true;
            }
            if (EnableStopAfterTime && (Stats.RunningTime+(long)( DateTime.Now - Stats.StartTime).TotalMilliseconds)> StopAfterTime)
            {
                reason = "Stop after "+ StopAfterTime.ToString("HH:mm:ss")+" run time condition triggered with "+ (new TimeSpan (0,0,0,0,(int)(Stats.RunningTime + (long)(DateTime.Now - Stats.StartTime).TotalMilliseconds))).ToString("HH:mm:ss")+" running time, Stopping.";
                return true;
            }
            if (win)
            {
                if (EnableStopAfterWinStreak && Stats.WinStreak >= StopAfterWinStreak)
                {
                    reason = string.Format("Stop after {0} {1} conditino triggered with {2} {1}, Stopping", StopAfterWinStreak, "Wines in a row", Stats.WinStreak);
                    return true;
                }
                if (EnableStopAfterBtcWin && -Stats.Profit >= StopAfterBtcWin)
                {
                    reason = string.Format("Stop after {0} {1} conditino triggered with {2} {1}, Stopping", StopAfterBtcWin, "Currency Win", Stats.Profit);
                    return true;
                }
                if (EnableStopAfterBtcWinStreak && Stats.CurrentProfit >= StopAfterBtcWinStreak)
                {
                    reason = string.Format("Stop after {0} {1} conditino triggered with {2} {1}, Stopping", StopAfterBtcWinStreak, "Currency Streak Win", Stats.CurrentProfit);
                    return true;
                }
                if (EnableStopAfterWins && Stats.Wins % StopAfterWins == 0)
                {
                    reason = string.Format("Stop after {0} {1} conditino triggered with {2} {1}, Stopping", StopAfterWins, "Wines", Stats.Wins);
                    return true;
                }
            }
            else
            {
                if (EnableStopAfterLoseStreak && Stats.LossStreak>= StopAfterLoseStreak)
                {
                    reason = string.Format("Stop after {0} {1} conditino triggered with {2} {1}, Stopping", StopAfterLoseStreak, "Losses in a row", Stats.LossStreak);
                    return true;
                }
                if (EnableStopAfterBtcLoss && -Stats.Profit >= StopAfterBtcLoss)
                {
                    reason = string.Format("Stop after {0} {1} conditino triggered with {2} {1}, Stopping", StopAfterBtcLoss, "Currency Loss", Stats.Profit);
                    return true;
                }
                if (EnableStopAfterBtcLoseStreak && Stats.CurrentProfit >= StopAfterBtcLoseStreak)
                {
                    reason = string.Format("Stop after {0} {1} conditino triggered with {2} {1}, Stopping", StopAfterBtcLoseStreak, "Currency Streak Loss", Stats.CurrentProfit);
                    return true;
                }
                if (EnableStopAfterLosses && Stats.Losses % StopAfterLosses ==0)
                {
                    reason = string.Format("Stop after {0} {1} conditino triggered with {2} {1}, Stopping", StopAfterLosses, "Losses", Stats.Losses);
                    return true;
                }

            }
            reason = "";
            return false;
        }

        public bool CheckWithdraw(DiceBet NewBet, bool win, SessionStats Stats,  out decimal Amount)
        {
            Amount = 0;
            if (EnableUpperBalanceLimit && UpperBalanceLimitAction == LimitAction.Withdraw)
            {
                //check balance
                //stop if balance is larger
                Amount = UpperBalanceLimitActionAmount;
            }
            if (EnableLowerBalanceLimit && LowerBalanceLimitAction == LimitAction.Withdraw)
            {
                //check balance
                //stop if balance is lower
                Amount = LowerBalanceLimitActionAmount;
            }
            if (EnableUpperProfitLimitt && UpperProfitLimitAction == LimitAction.Withdraw)
            {
                if (Stats.Profit >= UpperProfitLimit)
                {
                    Amount = UpperProfitLimitActionAmount;
                    return true;
                }
            }
            if (EnableLowerProfitLimit && LowerProfitLimitAction == LimitAction.Withdraw)
            {
                if (Stats.Profit <= LowerProfitLimit)
                {
                    Amount = LowerProfitLimitActionAmount;
                    return true;
                }
            }
            return false;
        }

        public bool CheckTips(DiceBet NewBet, bool win, SessionStats Stats, out decimal Amount)
        {
           Amount = 0;
            Amount = 0;
            if (EnableUpperBalanceLimit && UpperBalanceLimitAction == LimitAction.Tip)
            {
                //check balance
                //stop if balance is larger
                Amount = UpperBalanceLimitActionAmount;
            }
            if (EnableLowerBalanceLimit && LowerBalanceLimitAction == LimitAction.Tip)
            {
                //check balance
                //stop if balance is lower
                Amount = LowerBalanceLimitActionAmount;
            }
            if (EnableUpperProfitLimitt && UpperProfitLimitAction == LimitAction.Tip)
            {
                if (Stats.Profit >= UpperProfitLimit)
                {
                    Amount = UpperProfitLimitActionAmount;
                    return true;
                }
            }
            if (EnableLowerProfitLimit && LowerProfitLimitAction == LimitAction.Tip)
            {
                if (Stats.Profit <= LowerProfitLimit)
                {
                    Amount = LowerProfitLimitActionAmount;
                    return true;
                }
            }
            
            return false;
        }

        public bool CheckHighLow(DiceBet NewBet, bool win, SessionStats Stats, out bool NewHigh)
        {
            NewHigh = false;
            
             if (EnableSwitchWins && Stats.Wins% SwitchWins==0)
            {
                NewHigh = !NewBet.High;
                return true;
            }
            if (EnableSwitchWinStreak && Stats.WinStreak % SwitchWinStreak == 0 && win)
            {
                NewHigh = !NewBet.High;
                return true;
            }
            if (EnableSwitchLosses && Stats.Losses % SwitchLosses == 0)
            {
                NewHigh = !NewBet.High;
                return true;
            }
            if (EnableSwitchLossStreak && Stats.LossStreak % SwitchLossStreak == 0 && !win)
            {
                NewHigh = !NewBet.High;
                return true;
            }
            if (EnableSwitchBets && Stats.Bets % SwitchBets == 0)
            {
                NewHigh = !NewBet.High;
                return true;
            }
            return false;
        }

        public bool CheckBank(DiceBet NewBet, bool win, SessionStats Stats, out decimal Amount)
        {
            Amount = 0;
            if (EnableUpperBalanceLimit && UpperBalanceLimitAction == LimitAction.Bank)
            {
                //check balance
                //stop if balance is larger
                Amount = UpperBalanceLimitActionAmount;
            }
            if (EnableLowerBalanceLimit && LowerBalanceLimitAction == LimitAction.Bank)
            {
                //check balance
                //stop if balance is lower
                Amount = LowerBalanceLimitActionAmount;
            }
            if (EnableUpperProfitLimitt && UpperProfitLimitAction == LimitAction.Bank)
            {
                if (Stats.Profit >= UpperProfitLimit)
                {
                    Amount = UpperProfitLimitActionAmount;
                    return true;
                }
            }
            if (EnableLowerProfitLimit && LowerProfitLimitAction == LimitAction.Bank)
            {
                if (Stats.Profit <= LowerProfitLimit)
                {
                    Amount = LowerProfitLimitActionAmount;
                    return true;
                }
            }
            return false;
        }

        public bool CheckResetSeed(DiceBet NewBet, bool win, SessionStats Stats)
        {
            if (EnableResetSeedBets && Stats.Bets%ResetSeedBets==0)
            {
                return true;
            }
            if (EnableResetSeedWins && Stats.Wins % ResetSeedWins == 0)
            {
                return true;
            }
            if (EnableResetSeedLosses && Stats.Losses % ResetSeedLosses == 0)
            {
                return true;
            }
            if (EnableResetSeedWinStreak && Stats.WinStreak % ResetSeedWinStreak == 0 && win)
            {
                return true;
            }
            if (EnableResetSeedLossStreak && Stats.LossStreak % ResetSeedLossStreak == 0 && !win)
            {
                return true;
            }
            return false;
        }

        //Process Bet
        #endregion
    }
}
