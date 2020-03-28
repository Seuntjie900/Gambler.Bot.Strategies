using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using Jint;

namespace DoormatBot.Strategies
{
    public class ProgrammerJS : BaseStrategy, ProgrammerMode
    {

        Engine Runtime;
        
        public string FileName { get; set; }


        public event EventHandler<WithdrawEventArgs> OnWithdraw;
        public event EventHandler<InvestEventArgs> OnInvest;
        public event EventHandler<TipEventArgs> OnTip;
        public event EventHandler<EventArgs> OnStop;
        public event EventHandler<EventArgs> OnResetSeed;
        public event EventHandler<PrintEventArgs> OnPrint;
        public event EventHandler<RunSimEventArgs> OnRunSim;
        public event EventHandler<EventArgs> OnResetStats;
        public event EventHandler<ReadEventArgs> OnRead;
        public event EventHandler<ReadEventArgs> OnReadAdv;
        public event EventHandler<EventArgs> OnAlarm;
        public event EventHandler<EventArgs> OnChing;
        public event EventHandler<EventArgs> OnResetBuiltIn;
        public event EventHandler<ExportSimEventArgs> OnExportSim;

        public override PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            PlaceDiceBet NextBet =  new PlaceDiceBet(PreviousBet.TotalAmount, PreviousBet.High, PreviousBet.Chance);
            //TypeReference.CreateTypeReference
            Runtime.Invoke("DoDiceBet", PreviousBet, Win, NextBet);
            return NextBet;
        }

        public void CreateRuntime()
        {
            Runtime = new Engine();
            
            Runtime.SetValue("Stats", Stats);
            Runtime.SetValue("Balance", Balance);
            Runtime.SetValue("Withdraw", (Action<string,decimal>)Withdraw);
            Runtime.SetValue("Invest", (Action< decimal>)Invest);
            Runtime.SetValue("Tip", (Action<string, decimal>)Tip);
            Runtime.SetValue("ResetSeed", (Action)ResetSeed);
            Runtime.SetValue("Print", (Action<string>)Print);
            Runtime.SetValue("RunSim", (Action < decimal, long>)RunSim);
            Runtime.SetValue("ResetStats", (Action)ResetStats);
            Runtime.SetValue("Read", (Func<string, int, object>)Read);
            Runtime.SetValue("Readadv", (Func<string, int,string,string,string, object> )Readadv);
            Runtime.SetValue("Alarm", (Action)Alarm);
            Runtime.SetValue("Ching", (Action)Ching);
            Runtime.SetValue("ResetBuiltIn", (Action)ResetBuiltIn);
            Runtime.SetValue("ExportSim", (Action<string>)ExportSim);
        }

        void withdraw(object sender, EventArgs e)
        {
            Logger.DumpLog("Ping!",0);
        }

        public void LoadScript()
        {
            Runtime.SetValue("Stats", Stats);
            Runtime.SetValue("Balance", Balance);
            string scriptBody = File.ReadAllText(FileName);

            Runtime.Execute(scriptBody);
        }

        public override PlaceDiceBet RunReset()
        {
            PlaceDiceBet NextBet = new PlaceDiceBet(0, false, 0);
            Runtime.Invoke("ResetDice", NextBet);
            return NextBet;
        }

        public void UpdateSessionStats(SessionStats Stats)
        {
            Runtime.SetValue("Stats", Stats);
            Runtime.SetValue("Balance", Balance);

        }

        public void UpdateSite(SiteDetails Stats)
        {
            Runtime.SetValue("SiteDetails", Stats);
        }

        public void UpdateSiteStats(SiteStats Stats)
        {
            Runtime.SetValue("SiteStats", Stats);
        }
        void Withdraw(string Address, decimal Amount)
        {
            OnWithdraw?.Invoke(this, new WithdrawEventArgs { Address = Address, Amount = Amount });
        }
        void Invest(decimal Amount)
        {
            OnInvest?.Invoke(this, new InvestEventArgs { Amount = Amount });
        }
        void Tip(string Receiver, decimal Amount)
        {
            OnTip?.Invoke(this, new TipEventArgs { Receiver = Receiver, Amount = Amount });
        }
        void ResetSeed()
        {
            OnResetSeed?.Invoke(this, new EventArgs());
        }
        void Print(string PrintValue)
        {
            OnPrint?.Invoke(this, new PrintEventArgs { Message = PrintValue });
        }
        void RunSim(decimal Balance, long Bets)
        {
            OnRunSim?.Invoke(this, new RunSimEventArgs { Balance = Balance, Bets = Bets });
        }
        void ResetStats()
        {
            OnResetStats?.Invoke(this, new EventArgs());
        }
        object Read(string prompt, int DataType)
        {
            ReadEventArgs tmpArgs = new ReadEventArgs { Prompt = prompt, DataType = DataType };
            OnRead?.Invoke(this, tmpArgs);
            return tmpArgs.Result;
        }
        object Readadv(string prompt, int DataType, string userinputext, string btncanceltext, string btnoktext)
        {
            ReadEventArgs tmpArgs = new ReadEventArgs { Prompt = prompt, DataType = DataType, userinputext = userinputext, btncanceltext = btncanceltext, btnoktext = btnoktext };
            OnReadAdv?.Invoke(this, tmpArgs);
            return tmpArgs.Result;
        }
        void Alarm()
        {
            OnAlarm?.Invoke(this, new EventArgs());
        }
        void Ching()
        {
            OnChing?.Invoke(this, new EventArgs());
        }
        void ResetBuiltIn()
        {
            OnResetBuiltIn?.Invoke(this, new EventArgs());
        }
        void ExportSim(string FileName)
        {
            OnExportSim?.Invoke(this, new ExportSimEventArgs { FileName = FileName });
        }
    }
}
