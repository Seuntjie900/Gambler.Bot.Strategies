using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.Common.Helpers;
using IronPython.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Scripting.Hosting;
using System;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Plinko;
using Gambler.Bot.Common.Games.Roulette;
using Gambler.Bot.Common.Games;

namespace Gambler.Bot.Strategies.Strategies
{
    public class ProgrammerPython: BaseStrategy, IProgrammerMode
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
        public event EventHandler<InvestEventArgs> OnBank;

        public event EventHandler<EventArgs> OnResetProfit;
        public event EventHandler<EventArgs> OnResetPartialProfit;

        public ProgrammerPython(ILogger logger) : base(logger)
        {

        }

        public ProgrammerPython()
        {
            
        }

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
        {
            try
            {
                PlaceBet NextBet = PreviousBet.CreateRetry();
                Scope.SetVariable("NextBet", NextBet);
                Scope.SetVariable("Win", Win);
                Scope.SetVariable("PreviousBet", PreviousBet);
                dynamic result = Scope.DoDiceBet(PreviousBet, Win, NextBet);
                
                return Scope.GetVariable("NextBet") as PlaceBet; ;
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
            }
            return null;
        }

        


        public void CreateRuntime()
        {
            //CurrentRuntime = Python.CreateRuntime();
            Engine = Python.CreateEngine();
            Scope = Engine.CreateScope();
            (Scope as ScriptScope).SetVariable("Bank", (Action<decimal>)Bank);
            (Scope as ScriptScope).SetVariable("Withdraw", (Action<string,decimal>)Withdraw);
            (Scope as ScriptScope).SetVariable("Invest", (Action< decimal>)Invest);
            (Scope as ScriptScope).SetVariable("Tip", (Action<string, decimal>)Tip);
            (Scope as ScriptScope).SetVariable("ResetSeed", (Action)ResetSeed);
            (Scope as ScriptScope).SetVariable("Print", (Action<string>)Print);
            (Scope as ScriptScope).SetVariable("RunSim", (Action < decimal, long, bool>)RunSim);
            (Scope as ScriptScope).SetVariable("ResetStats", (Action)ResetStats);
            (Scope as ScriptScope).SetVariable("Read", (Func<string, int, object>)Read);
            (Scope as ScriptScope).SetVariable("Readadv", (Func<string, int,string,string,string, object> )Readadv);
            (Scope as ScriptScope).SetVariable("Alarm", (Action)Alarm);
            (Scope as ScriptScope).SetVariable("Ching", (Action)Ching);
            (Scope as ScriptScope).SetVariable("ResetBuiltIn", (Action)ResetBuiltIn);
            (Scope as ScriptScope).SetVariable("ExportSim", (Action<string>)ExportSim);
            (Scope as ScriptScope).SetVariable("Stop", (Action)_Stop);
            (Scope as ScriptScope).SetVariable("SetCurrency", (Action<string>)SetCurrency);
            (Scope as ScriptScope).SetVariable("SetCurrency", (Func<string,PlaceBet>)ChangeGame);
        }                                      

        public void LoadScript()
        {
             Scope.SetVariable("Stats", Stats);
             Scope.SetVariable("Balance", Balance);
             var source = Engine.CreateScriptSourceFromFile(FileName);
             CompCode = source.Compile();
             dynamic result = CompCode.Execute(Scope);
            
        }

        public override PlaceBet RunReset(Games Game)
        {
            PlaceBet NextBet = CreateEmptyPlaceBet(Game);

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
            Scope.SetVariable("Balance",Stats.Balance);
        }
        public void SetSimulation(bool IsSimulation)
        {
            Scope.SetVariable("InSimulation",IsSimulation);
        }
        void Bank(decimal Amount)
        {
            OnBank?.Invoke(this, new InvestEventArgs { Amount = Amount });
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
            OnRunSim?.Invoke(this, new RunSimEventArgs { Balance = Balance, Bets = Bets, WriteLog=log });
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
        public PlaceBet ChangeGame(string Game)
        {
            var tmp = CreateEmptyPlaceBet(Enum.Parse<Games>(Game));
            var nextbet = Scope.GetVariable("NextBet") as PlaceBet;
            tmp.Amount = nextbet?.Amount ?? 0;
            Scope.SetVariable("NextBet", tmp);
            return tmp;
        }
    }
}
