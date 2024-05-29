﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gambler.Bot.Core.Games;
using Gambler.Bot.AutoBet.Helpers;
using IronPython.Runtime;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using Microsoft.Extensions.Logging;
using Doormat.Bot.Helpers;
using Gambler.Bot.Core.Sites.Classes;

namespace Gambler.Bot.AutoBet.Strategies
{
    public class ProgrammerPython: BaseStrategy, ProgrammerMode, iDiceStrategy
    {
        public override string StrategyName { get; protected set; } = "ProgrammerPython";
        public string FileName { get; set; }
        public bool High { get ; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set ; }
        public decimal StartChance { get ; set ; }

        ScriptRuntime CurrentRuntime;
        
        ScriptEngine Engine;
        dynamic Scope;
        CompiledCode CompCode;

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
        public event EventHandler<PrintEventArgs> OnScriptError;
        public event EventHandler<PrintEventArgs> OnSetCurrency;

        public ProgrammerPython(ILogger logger) : base(logger)
        {

        }

        public ProgrammerPython()
        {
            
        }

        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            try
            {
                PlaceDiceBet NextBet = new PlaceDiceBet(PreviousBet.TotalAmount, PreviousBet.High, PreviousBet.Chance);

                dynamic result = Scope.DoDiceBet(PreviousBet, Win, NextBet);

                return NextBet;
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
            }
            return null;
        }

        public override PlaceCrashBet CalculateNextCrashBet(CrashBet PreviousBet, bool Win)
        {
            PlaceCrashBet NextBet = new PlaceCrashBet();

            dynamic result = Scope.DoCrashBet(PreviousBet, Win, NextBet);

            return result;
        }

        public override PlacePlinkoBet CalculateNextPlinkoBet(PlinkoBet PreviousBet, bool Win)
        {
            PlacePlinkoBet NextBet = new PlacePlinkoBet();

            dynamic result = Scope.DoPlinkoBet(PreviousBet, Win, NextBet);

            return result;
        }

        public override PlaceRouletteBet CalculateNextRouletteBet(RouletteBet PreviousBet, bool Win)
        {
            PlaceRouletteBet NextBet = new PlaceRouletteBet();

            dynamic result = Scope.DoRouletteBet(PreviousBet, Win, NextBet);

            return result;
        }


        public void CreateRuntime()
        {
            //CurrentRuntime = Python.CreateRuntime();
            Engine = Python.CreateEngine();
            Scope = Engine.CreateScope();
            (Scope as ScriptScope).SetVariable("Withdraw", (Action<string,decimal>)Withdraw);
            (Scope as ScriptScope).SetVariable("Invest", (Action< decimal>)Invest);
            (Scope as ScriptScope).SetVariable("Tip", (Action<string, decimal>)Tip);
            (Scope as ScriptScope).SetVariable("ResetSeed", (Action)ResetSeed);
            (Scope as ScriptScope).SetVariable("Print", (Action<string>)Print);
            (Scope as ScriptScope).SetVariable("RunSim", (Action < decimal, long>)RunSim);
            (Scope as ScriptScope).SetVariable("ResetStats", (Action)ResetStats);
            (Scope as ScriptScope).SetVariable("Read", (Func<string, int, object>)Read);
            (Scope as ScriptScope).SetVariable("Readadv", (Func<string, int,string,string,string, object> )Readadv);
            (Scope as ScriptScope).SetVariable("Alarm", (Action)Alarm);
            (Scope as ScriptScope).SetVariable("Ching", (Action)Ching);
            (Scope as ScriptScope).SetVariable("ResetBuiltIn", (Action)ResetBuiltIn);
            (Scope as ScriptScope).SetVariable("ExportSim", (Action<string>)ExportSim);
            (Scope as ScriptScope).SetVariable("Stop", (Action)_Stop);
            (Scope as ScriptScope).SetVariable("SetCurrency", (Action<string>)SetCurrency);
        }                                      

        public void LoadScript()
        {
             Scope.SetVariable("Stats", Stats);
             Scope.SetVariable("Balance", Balance);
             var source = Engine.CreateScriptSourceFromFile(FileName);
             CompCode = source.Compile();
             dynamic result = CompCode.Execute(Scope);
            
        }

        public override PlaceDiceBet RunReset()
        {
            PlaceDiceBet NextBet = new PlaceDiceBet(0,false,0);

            dynamic result = Scope.ResetDice(NextBet);

            return NextBet;
        }

        public override void OnError(BotErrorEventArgs e)
        {
            dynamic result = Scope.OnError(e);
        }

        public void UpdateSessionStats(SessionStats Stats)
        {
            Scope.SetVariable("Stats", Stats);
        }

        public void UpdateSite(SiteDetails Stats)
        {
            Scope.SetVariable("SiteDetails", Stats);
        }

        public void UpdateSiteStats(SiteStats Stats)
        {
            Scope.SetVariable("SiteStats", Stats);
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

        public void ExecuteCommand(string Command)
        {
            Engine.Execute(Command);
        }
        
        public void _Stop()
        {
            CallStop("Stop() function called from programmer mode.");
        }
        private void SetCurrency(string newCurrency)
        {
            OnSetCurrency?.Invoke(this, new PrintEventArgs { Message = newCurrency });
        }
    }
}
