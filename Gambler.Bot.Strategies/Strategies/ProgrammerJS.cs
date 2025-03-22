using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.Common.Helpers;
using Jint;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games;

namespace Gambler.Bot.Strategies.Strategies
{
    public class ProgrammerJS : BaseStrategy, IProgrammerMode
    {
        public override string StrategyName { get; protected set; } = "ProgrammerJS";
        Engine Runtime;
        
        public string FileName { get; set; }
        public bool High { get ; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set ; }
        public decimal StartChance { get ; set ; }

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

        public ProgrammerJS(ILogger logger):base(logger)
        {
            
        }
        public ProgrammerJS()
        {
            
        }

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
        {
            try
            {
                PlaceBet NextBet = PreviousBet.CreateRetry();
                //TypeReference.CreateTypeReference
                Runtime.Invoke("CalculateBet", PreviousBet, Win, NextBet);
                return NextBet;
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
            }
            return null;
        }

        public void OnError(BotErrorEventArgs e)
        {
            try
            {
                //TypeReference.CreateTypeReference
                Runtime.Invoke("OnError", e);
            }
            catch (Exception ex)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
            }
            
        }

        public void CreateRuntime()
        {
            Runtime = new Engine();
            
            Runtime.SetValue("Stats", Stats);
            Runtime.SetValue("Balance", Balance);
            Runtime.SetValue("Withdraw", (Action<string,decimal>)Withdraw);
            Runtime.SetValue("Invest", (Action< decimal>)Invest);
            Runtime.SetValue("Bank", (Action<decimal>)Bank);
            Runtime.SetValue("Tip", (Action<string, decimal>)Tip);
            Runtime.SetValue("ResetSeed", (Action)ResetSeed);
            Runtime.SetValue("Print", (Action<string>)Print);
            Runtime.SetValue("RunSim", (Action < decimal, long, bool>)RunSim);
            Runtime.SetValue("ResetStats", (Action)ResetStats);
            Runtime.SetValue("Read", (Func<string, int, object>)Read);
            Runtime.SetValue("Readadv", (Func<string, int,string,string,string, object> )Readadv);
            Runtime.SetValue("Alarm", (Action)Alarm);
            Runtime.SetValue("Ching", (Action)Ching);
            Runtime.SetValue("ResetBuiltIn", (Action)ResetBuiltIn);
            Runtime.SetValue("ExportSim", (Action<string>)ExportSim);
            Runtime.SetValue("Stop", (Action)_Stop);
            Runtime.SetValue("SetCurrency", (Action<string>)SetCurrency);
        }

        void withdraw(object sender, EventArgs e)
        {
            _Logger?.LogDebug("Ping!");
        }

        public void LoadScript()
        {
            Runtime.SetValue("Stats", Stats);
            Runtime.SetValue("Balance", Balance);
            string scriptBody = File.ReadAllText(FileName);

            Runtime.Execute(scriptBody);
        }

        public override PlaceBet RunReset(Games Game)
        {
            PlaceBet NextBet = CreateEmptyPlaceBet(Game);
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
        public void SetSimulation(bool IsSimulation)
        {
            Runtime.SetValue("InSimulation", IsSimulation);
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
            try
            {
                Runtime.Execute(Command);
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
            }
        }
        public void _Stop()
        {
            CallStop("Stop() function called in programmer mode.");
        }
        private void SetCurrency(string newCurrency)
        {
            OnSetCurrency?.Invoke(this, new PrintEventArgs { Message = newCurrency });
        }
    }
}
