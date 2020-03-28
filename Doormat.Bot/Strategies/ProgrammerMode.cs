using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatBot.Strategies
{
    public interface ProgrammerMode
    {
        void CreateRuntime();
        void UpdateSessionStats(SessionStats Stats);
        void UpdateSiteStats(SiteStats Stats);
        void UpdateSite(SiteDetails Stats);
        void LoadScript();


        event EventHandler<WithdrawEventArgs> OnWithdraw;
        event EventHandler<InvestEventArgs> OnInvest;
        event EventHandler<TipEventArgs> OnTip;
        event EventHandler<EventArgs> OnStop;
        event EventHandler<EventArgs> OnResetSeed;
        event EventHandler<PrintEventArgs> OnPrint;
        /*event EventHandler<EventArgs> OnMartingale;
        event EventHandler<EventArgs> OnLabouchere;*/
        event EventHandler<RunSimEventArgs> OnRunSim;
        event EventHandler<EventArgs> OnResetStats;
        event EventHandler<ReadEventArgs> OnRead;
        event EventHandler<ReadEventArgs> OnReadAdv;
        event EventHandler<EventArgs> OnAlarm;
        event EventHandler<EventArgs> OnChing;
        //event EventHandler<ResetBuiltInEventArgs> OnResetBuiltIn;
        event EventHandler<ExportSimEventArgs> OnExportSim;

        
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
    public class RunSimEventArgs : EventArgs
    {
        public decimal Balance { get; set; }
        public long Bets { get; set; }
    }
    public class ReadEventArgs: EventArgs
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
