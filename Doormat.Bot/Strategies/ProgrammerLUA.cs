using Gambler.Bot.AutoBet.Helpers;
using Gambler.Bot.Core.Games;
using Gambler.Bot.Core.Sites.Classes;
using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using System;

namespace Gambler.Bot.AutoBet.Strategies
{
    public class ProgrammerLUA : BaseStrategy, ProgrammerMode, iDiceStrategy
    {
        public override string StrategyName { get; protected set; } = "ProgrammerLUA";
        public string FileName { get; set; }
        public bool High { get ; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set ; }
        public decimal StartChance { get ; set ; }

        Script CurrentRuntime = null;

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

        public ProgrammerLUA(ILogger logger) : base(logger)
        {

        }
        public ProgrammerLUA()
        {
            
        }

        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            try
            {
                PlaceDiceBet NextBet = new PlaceDiceBet(PreviousBet.TotalAmount, PreviousBet.High, PreviousBet.Chance);
                DynValue DoDiceBet = CurrentRuntime.Globals.Get("DoDiceBet");
                if (DoDiceBet != null)
                {
                    DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
                }
                else
                {
                    DoDiceBet = CurrentRuntime.Globals.Get("DoBet");
                    if (DoDiceBet != null)
                    {
                        DynValue Result = CurrentRuntime.Call(DoDiceBet);
                        NextBet.Chance = (decimal)CurrentRuntime.Globals["chance"];
                        NextBet.Amount = (decimal)CurrentRuntime.Globals["nextbet"];
                        NextBet.High = (bool)CurrentRuntime.Globals["bethigh"];
                    }
                }
                return NextBet;
            }
            catch (InternalErrorException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message=e.DecoratedMessage });
                //throw e;
            }
            catch (SyntaxErrorException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (ScriptRuntimeException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
               // throw e;
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
               // throw e;
            }
            return null;
        }

        public override void OnError(BotErrorEventArgs ErrorDetails)
        {            
            DynValue CallError = CurrentRuntime.Globals.Get("OnError");
            if (CallError != null)
            {
                DynValue Result = CurrentRuntime.Call(CallError, ErrorDetails);
            }            
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
            CurrentRuntime.Globals["Stop"] = (Action)_Stop;
            CurrentRuntime.Globals["SetCurrency"] = (Action<string>)SetCurrency;
        }

        public void LoadScript()
        {

            try
            {
                CurrentRuntime.Globals["Stats"] = Stats;
                CurrentRuntime.Globals["Balance"] = this.Balance;
                CurrentRuntime.DoFile(FileName);
            }
            catch (InternalErrorException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (SyntaxErrorException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (ScriptRuntimeException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
                //throw e;
            }
        }

        public override PlaceDiceBet RunReset()
        {
            try
            {
                DynValue DoDiceBet = CurrentRuntime.Globals.Get("ResetDice");
                if (DoDiceBet != null)
                {
                    PlaceDiceBet NextBet = new PlaceDiceBet(0, false, 0);
                    DynValue Result = CurrentRuntime.Call(DoDiceBet, NextBet);
                    return NextBet;
                }
            }
            catch (InternalErrorException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (SyntaxErrorException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (ScriptRuntimeException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
                //throw e;
            }
            return null;
        }

        public void UpdateSessionStats(SessionStats Stats)
        {
            CurrentRuntime.Globals["Stats"] = Stats;
            CurrentRuntime.Globals["Balance"] = this.Balance;
            SetLegacyVars();
        }

        public void UpdateSite(SiteDetails Details)
        {
            CurrentRuntime.Globals["SiteDetails"] = Details;
            CurrentRuntime.Globals["site"] = Details;
            CurrentRuntime.Globals["currencies"] = Details.Currencies;
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
        void _Stop()
        {
            CallStop("Stop function used in programmer mode");
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

        public void ExecuteCommand(string Command)
        {
            try
            {
                CurrentRuntime.DoString(Command);
            }
            catch (InternalErrorException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (SyntaxErrorException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (ScriptRuntimeException e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.DecoratedMessage });
                //throw e;
            }
            catch (Exception e)
            {
                OnScriptError?.Invoke(this, new PrintEventArgs { Message = e.ToString() });
                //throw e;
            }
        }
        public void SetCurrency(string newCurrency)
        {               
            OnSetCurrency?.Invoke(this, new PrintEventArgs { Message = newCurrency });
        }

        void SetLegacyVars()
        {
            try
            {
                
                //Lua.clear();
                CurrentRuntime.Globals["balance"] = this.Balance;
                CurrentRuntime.Globals["profit"] = this.Stats.Profit;
                CurrentRuntime.Globals["currentstreak"] = (this.Stats.WinStreak > 0) ? this.Stats.WinStreak : -this.Stats.LossStreak;
                CurrentRuntime.Globals["previousbet"] = Amount;
                CurrentRuntime.Globals["nextbet"] = Amount;
                CurrentRuntime.Globals["chance"] = Chance;
                CurrentRuntime.Globals["bethigh"] = High;
                CurrentRuntime.Globals["bets"] = this.Stats.Wins + this.Stats.Losses;
                CurrentRuntime.Globals["wins"] = this.Stats.Wins;
                CurrentRuntime.Globals["losses"] = this.Stats.Losses;
                CurrentRuntime.Globals["wagered"] = Stats.Wagered;


            }
            catch (Exception e)
            {
                _Logger?.LogError(e.Message, 1);
                _Logger?.LogError(e.StackTrace, 2);
                CallStop("LUA ERROR!!");
                Print("LUA ERROR!!");
                Print(e.Message);
            }
        }
    }
}
