using System;
using System.Collections.Generic;
using System.Text;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using MoonSharp.Interpreter;

namespace DoormatBot.Strategies
{
    public class ProgrammerLUA : BaseStrategy, ProgrammerMode
    {
        public string FileName { get; set; }

        Script CurrentRuntime = null;

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
            PlaceDiceBet NextBet = new PlaceDiceBet(PreviousBet.TotalAmount, PreviousBet.High, PreviousBet.Chance);
            DynValue DoDiceBet = CurrentRuntime.Globals.Get("DoDiceBet");
            if (DoDiceBet != null)
            {
                DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
            }
            return NextBet;
        }

        public override PlaceCrashBet CalculateNextCrashBet(CrashBet PreviousBet, bool Win)
        {
            PlaceCrashBet NextBet = new PlaceCrashBet();
            DynValue DoDiceBet = CurrentRuntime.Globals.Get("DoCrashBet");
            if (DoDiceBet != null)
            {
                DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
            }
            return NextBet;
        }

        public override PlacePlinkoBet CalculateNextPlinkoBet(PlinkoBet PreviousBet, bool Win)
        {
            PlacePlinkoBet NextBet = new PlacePlinkoBet();
            DynValue DoDiceBet = CurrentRuntime.Globals.Get("DoPlinkoBet");
            if (DoDiceBet != null)
            {
                DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
            }
            return NextBet;
        }

        public override PlaceRouletteBet CalculateNextRouletteBet(RouletteBet PreviousBet, bool Win)
        {
            PlaceRouletteBet NextBet = new PlaceRouletteBet();
            DynValue DoDiceBet = CurrentRuntime.Globals.Get("DoRouletteBet");
            if (DoDiceBet != null)
            {
                DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
            }
            return NextBet;
        }
       

        public void CreateRuntime()
        {
            CurrentRuntime = new Script();
            //UserData.RegisterAssembly();
            UserData.RegisterType<SessionStats>();
            UserData.RegisterType < PlaceDiceBet>();
            UserData.RegisterType < DiceBet>();
            UserData.RegisterType < SiteDetails>();
            UserData.RegisterType < SiteStats>();
            UserData.RegisterType < CrashBet>();
            UserData.RegisterType < PlaceCrashBet>();
            UserData.RegisterType < PlinkoBet>();
            UserData.RegisterType < PlacePlinkoBet>();
            UserData.RegisterType < PlaceRouletteBet>();
            UserData.RegisterType < RouletteBet>();
            CurrentRuntime.Globals["Withdraw"] = (Action<string,decimal>)Withdraw;
            CurrentRuntime.Globals["Invest"] = (Action< decimal>)Invest;
            CurrentRuntime.Globals["Tip"] = (Action<string, decimal>)Tip;
            CurrentRuntime.Globals["ResetSeed"] = (Action)ResetSeed;
            CurrentRuntime.Globals["Print"] = (Action<string>)Print;
            CurrentRuntime.Globals["RunSim"] = (Action < decimal, long>)RunSim;
            CurrentRuntime.Globals["ResetStats"] = (Action)ResetStats;
            CurrentRuntime.Globals["Read"] = (Func<string, int, object>)Read;
            CurrentRuntime.Globals["Readadv"] = (Func<string, int,string,string,string, object> )Readadv;
            CurrentRuntime.Globals["Alarm"] = (Action)Alarm;
            CurrentRuntime.Globals["Ching"] = (Action)Ching;
            CurrentRuntime.Globals["ResetBuiltIn"] = (Action)ResetBuiltIn;
            CurrentRuntime.Globals["ExportSim"] = (Action<string>)ExportSim;
            CurrentRuntime.Globals["Stop"] = (Action)Stop;
        }

        public void LoadScript()
        {
            

            CurrentRuntime.Globals["Stats"] = Stats;
            CurrentRuntime.Globals["Balance"] = this.Balance;             
            CurrentRuntime.DoFile(FileName);
        }

        public override PlaceDiceBet RunReset()
        {
            DynValue DoDiceBet = CurrentRuntime.Globals.Get("ResetDice");
            if (DoDiceBet!=null)
            {
                PlaceDiceBet NextBet = new PlaceDiceBet(0,false,0);
                DynValue Result = CurrentRuntime.Call(DoDiceBet, NextBet);
                return NextBet;
            }
            return null;
        }

        public void UpdateSessionStats(SessionStats Stats)
        {
            CurrentRuntime.Globals["Stats"] = Stats;
            CurrentRuntime.Globals["Balance"] = this.Balance;

        }

        public void UpdateSite(SiteDetails Details)
        {
            CurrentRuntime.Globals["SiteDetails"] = Details;
        }

        public void UpdateSiteStats(SiteStats Stats)
        {
            CurrentRuntime.Globals["SiteStats"] = Stats;
        }

        void Withdraw(string Address, decimal Amount)
        {
            OnWithdraw?.Invoke(this, new WithdrawEventArgs { Address=Address, Amount=Amount });            
        }
        void Invest(decimal Amount)
        {
            OnInvest?.Invoke(this, new InvestEventArgs { Amount=Amount });
        }
        void Tip(string Receiver, decimal Amount)
        {
            OnTip?.Invoke(this, new TipEventArgs { Receiver=Receiver, Amount=Amount });
        }        
        void ResetSeed()
        {
            OnResetSeed?.Invoke(this, new EventArgs());
        }
        void Print(string PrintValue)
        {
            OnPrint?.Invoke(this, new PrintEventArgs {  Message=PrintValue});
        }
        void RunSim(decimal Balance, long Bets)
        {
            OnRunSim?.Invoke(this, new RunSimEventArgs { Balance=Balance, Bets=Bets });
        }
        void ResetStats()
        {
            OnResetStats?.Invoke(this, new EventArgs());
        }
        void Stop()
        {
            OnStop?.Invoke(this, new EventArgs());
        }
        object Read(string prompt, int DataType)
        {
            ReadEventArgs tmpArgs = new ReadEventArgs { Prompt= prompt, DataType= DataType };
            OnRead?.Invoke(this, tmpArgs);
            return tmpArgs.Result;
        }
        object Readadv(string prompt, int DataType, string userinputext, string btncanceltext, string btnoktext)
        {
            ReadEventArgs tmpArgs = new ReadEventArgs { Prompt = prompt, DataType = DataType, userinputext=userinputext, btncanceltext=btncanceltext, btnoktext=btnoktext };
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
            OnExportSim?.Invoke(this, new ExportSimEventArgs { FileName = FileName});
        }
    }
}
