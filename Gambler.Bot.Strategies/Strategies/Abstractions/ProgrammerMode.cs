using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Common.Helpers;
using System;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games;

namespace Gambler.Bot.Strategies.Strategies.Abstractions
{
    public interface IProgrammerMode
    {
        void CreateRuntime();
        void UpdateSessionStats(SessionStats Stats);
        void UpdateSiteStats(SiteStats Stats);
        void UpdateSite(SiteDetails Stats, string currency);
        void SetSimulation(bool IsSimulation);
        void LoadScript();
        void ExecuteCommand(string Command);

        event EventHandler<InvestEventArgs> OnBank;
        event EventHandler<WithdrawEventArgs> OnWithdraw;
        event EventHandler<InvestEventArgs> OnInvest;
        event EventHandler<TipEventArgs> OnTip;
        event EventHandler<EventArgs> OnResetSeed;
        event EventHandler<PrintEventArgs> OnPrint;
        event EventHandler<RunSimEventArgs> OnRunSim;
        event EventHandler<EventArgs> OnResetStats;
        event EventHandler<ReadEventArgs> OnRead;
        event EventHandler<ReadEventArgs> OnReadAdv;
        event EventHandler<EventArgs> OnAlarm;
        event EventHandler<EventArgs> OnChing;
        event EventHandler<ExportSimEventArgs> OnExportSim;
        event EventHandler<PrintEventArgs> OnScriptError;
        event EventHandler<PrintEventArgs> OnSetCurrency;
        event EventHandler<EventArgs> OnResetProfit;
        event EventHandler<EventArgs> OnResetPartialProfit;
        public string FileName { get; set; }
    }

    public class WithdrawEventArgs : EventArgs
    {
        public string Address { get; set; }
        public decimal Amount { get; set; }
    }
    public class InvestEventArgs : EventArgs
    {
        public decimal Amount { get; set; }
    }
    public class TipEventArgs : EventArgs
    {
        public string Receiver { get; set; }
        public decimal Amount { get; set; }
    }
    public class PrintEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
    public class GameChangedArgs : EventArgs
    {
        public Games Game { get; set; }
    }
    public class RunSimEventArgs : EventArgs
    {
        public decimal Balance { get; set; }
        public long Bets { get; set; }
        public bool WriteLog { get; set; }
    }
    public class ReadEventArgs : EventArgs
    {
        public string Prompt { get; set; }
        public int DataType { get; set; }
        public string userinputext { get; set; }
        public string btncanceltext { get; set; }
        public string btnoktext { get; set; }
        public object Result { get; set; }
    }
    public class ExportSimEventArgs : EventArgs
    {
        public string FileName { get; set; }
    }
    public class ResetBuiltInEventArgs : EventArgs
    {
        public PlaceDiceBet NewBet { get; set; }
    }
}
