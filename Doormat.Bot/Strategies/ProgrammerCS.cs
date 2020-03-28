using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using GlobalsObject;

namespace DoormatBot.Strategies
{
    public class ProgrammerCS : BaseStrategy, ProgrammerMode
    {
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

        ScriptState runtime;
        Globals globals;
        Script DoDiceBet = null;
        Script ResetDice = null;
        public override PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            PlaceDiceBet NextBet = new PlaceDiceBet(PreviousBet.TotalAmount, PreviousBet.High, PreviousBet.Chance);
            
            globals.NextDiceBet = NextBet;
            globals.PreviousDiceBet = PreviousBet;
            globals.DiceWin =Win;
            if (DoDiceBet == null)
            {
                runtime = runtime.ContinueWithAsync("DoDiceBet(PreviousDiceBet, DiceWin, NextDiceBet)").Result;
                DoDiceBet = runtime.Script;
            }
            /*runtime = runtime.ContinueWithAsync("DoDiceBet(PreviousDiceBet, DiceWin, NextDiceBet)", ScriptOptions.Default.WithReferences(
                    Assembly.GetExecutingAssembly())
                    .WithImports(
                        "DoormatBot",
                        "DoormatBot.Games",
                        "System")).Result;*/


            //;
            else
                runtime = DoDiceBet.RunFromAsync(runtime).Result;
            return NextBet;
        }
        delegate void dDoDiceBet(DiceBet PreviousBet, bool Win, PlaceDiceBet NextBet);

        public void CreateRuntime()
        {
            var script = CSharpScript.Create("Console.WriteLine(\"Starting C# Programmer mode\");",
                ScriptOptions.Default.WithReferences(
                    Assembly.GetExecutingAssembly())
                    .WithImports(
                        "DoormatBot", 
                        "DoormatBot.Games",
                        "System"), 
                typeof(Globals));

            globals = new Globals() {
                Stats =Stats,
                Balance = Balance,
                Withdraw=Withdraw,
                 Invest=Invest,
                 Tip=Tip,
                 ResetSeed=ResetSeed,
                 Print=Print,
                 RunSim=RunSim,
                 ResetStats=ResetStats,
                 Read=Read,
                 Readadv=Readadv,
                 Alarm=Alarm,
                 Ching=Ching,
                 ResetBuiltIn=ResetBuiltIn,
                 ExportSim =ExportSim
            };
            runtime = script.RunAsync(globals: globals).Result;
        }

        public void LoadScript()
        {
            string scriptBody = File.ReadAllText(FileName);
            runtime = runtime.Script.ContinueWith(scriptBody).RunFromAsync(runtime).Result;
            DoDiceBet = null;
            globals.Stats = Stats;
            globals.Balance = Balance;
        }

        public override PlaceDiceBet RunReset()
        {
            PlaceDiceBet NextBet = new PlaceDiceBet(0, false, 0);
            globals.NextDiceBet = NextBet;            
            if (ResetDice == null)
            {
                runtime = runtime.ContinueWithAsync("ResetDice(NextDiceBet)").Result;
                ResetDice = runtime.Script;
            }
            
            else
                runtime = ResetDice.RunFromAsync(runtime).Result;
            return NextBet;
        }

        public void UpdateSessionStats(SessionStats Stats)
        {
            globals.Stats = Stats;
        }

        public void UpdateSite(SiteDetails Stats)
        {
            globals.Balance= Balance;
            globals.SiteDetails = Stats;
        }

        public void UpdateSiteStats(SiteStats Stats)
        {
            globals.SiteStats = Stats;
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
