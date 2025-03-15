using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;

namespace Gambler.Bot.Strategies.Strategies
{
    public class ProgrammerCS : BaseStrategy, IProgrammerMode, iDiceStrategy
    {
        public override string StrategyName { get; protected set; } = "ProgrammerCS";
        public string FileName { get; set; }
        public bool High { get ; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set ; }
        public decimal StartChance { get ; set ; }

        public event EventHandler<WithdrawEventArgs> OnWithdraw;
        public event EventHandler<InvestEventArgs> OnInvest;
        public event EventHandler<TipEventArgs> OnTip;
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

        ScriptState runtime;
        Globals globals;
        Script DoDiceBet = null;
        Script ResetDice = null;

        public ProgrammerCS(ILogger logger) : base(logger)
        {

        }
        public ProgrammerCS()
        {
            
        }
        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            try
            {
                PlaceDiceBet NextBet = new PlaceDiceBet(PreviousBet.TotalAmount, PreviousBet.High, PreviousBet.Chance);

                globals.NextDiceBet = NextBet;
                globals.PreviousDiceBet = PreviousBet;
                globals.DiceWin = Win;
                //if (DoDiceBet == null)
                {

                    runtime = runtime.ContinueWithAsync("DoDiceBet(PreviousDiceBet, DiceWin, NextDiceBet)").Result;
                    DoDiceBet = runtime.Script;
                }
                /*else
                runtime = runtime.ContinueWithAsync("DoDiceBet(PreviousDiceBet, DiceWin, NextDiceBet)", ScriptOptions.Default.WithReferences(
                        Assembly.GetExecutingAssembly())
                        .WithImports(
                            "Gambler.Bot.Strategies",
                            "Gambler.Bot.Strategies.Games",
                            "System")).Result;*/


                //;
                /*else                
                    runtime = DoDiceBet.RunFromAsync(runtime).Result;*/
                return NextBet;
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
            }
            return null;
        }
        delegate void dDoDiceBet(DiceBet PreviousBet, bool Win, PlaceDiceBet NextBet);

        public void CreateRuntime()
        {
            var script = CSharpScript.Create("Console.WriteLine(\"Starting C# Programmer mode\");",
                ScriptOptions.Default.WithReferences(
                    Assembly.GetExecutingAssembly())
                    .WithImports(
                        "Gambler.Bot.Strategies",
                        "Gambler.Bot.Common.Games",
                        "System"), 
                typeof(Globals));

            globals = new Globals() {
                Stats = Stats,
                Balance = Balance,
                Withdraw = Withdraw,
                Invest = Invest,
                Tip = Tip,
                ResetSeed = ResetSeed,
                Print = Print,
                RunSim = RunSim,
                ResetStats = ResetStats,
                Read = Read,
                Readadv = Readadv,
                Alarm = Alarm,
                Ching = Ching,
                ResetBuiltIn = ResetBuiltIn,
                ExportSim = ExportSim,
                Stop = _Stop,
                SetCurrency = SetCurrency
                 
            };
            runtime = script.RunAsync(globals: globals).Result;
            
        }

        private void SetCurrency(string newCurrency)
        {
            OnSetCurrency?.Invoke(this, new PrintEventArgs { Message = newCurrency });
        }

        public void LoadScript()
        {
            string scriptBody = File.ReadAllText(FileName);
            runtime = runtime.Script.ContinueWith(scriptBody).RunFromAsync(runtime).Result;
            DoDiceBet = null;
            globals.Stats = Stats;
            globals.Balance = Balance;
        }

        public override void OnError(BotErrorEventArgs ErrorDetails)
        {
            
            globals.ErrorArgs = ErrorDetails;
            //if (ResetDice == null)
            {
                runtime = runtime.ContinueWithAsync("OnError(ErrorArgs)").Result;
                ResetDice = runtime.Script;
            }

            
        }

        public override PlaceDiceBet RunReset()
        {
            PlaceDiceBet NextBet = new PlaceDiceBet(0, false, 0);
            globals.NextDiceBet = NextBet;            
            //if (ResetDice == null)
            {
                runtime = runtime.ContinueWithAsync("ResetDice(NextDiceBet)").Result;
                ResetDice = runtime.Script;
            }
            
            //else
            //    runtime = ResetDice.RunFromAsync(runtime).Result;
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

        public void SetSimulation(bool IsSimulation)
        {
            globals.InSimulation = IsSimulation;
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
        void RunSim(decimal Balance, long Bets, bool log)
        {
            OnRunSim?.Invoke(this, new RunSimEventArgs { Balance = Balance, Bets = Bets, WriteLog = log });
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
            runtime = runtime.ContinueWithAsync(Command).Result;
        }
        public void _Stop()
        {
            CallStop("Stop() function called from Programmer Mode");
        }
    }
   
}
