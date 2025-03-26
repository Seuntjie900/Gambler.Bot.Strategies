using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.Common.Helpers;
using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using System;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Plinko;
using Gambler.Bot.Common.Games.Roulette;
using Gambler.Bot.Common.Games;
using static IronPython.Modules._ast;
using Mono.Unix.Native;

namespace Gambler.Bot.Strategies.Strategies
{
    public class ProgrammerLUA : BaseStrategy, IProgrammerMode
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
        public event EventHandler<EventArgs> OnResetProfit;
        public event EventHandler<EventArgs> OnResetPartialProfit;
        public event EventHandler<ReadEventArgs> OnRead;
        public event EventHandler<ReadEventArgs> OnReadAdv;
        public event EventHandler<EventArgs> OnAlarm;
        public event EventHandler<EventArgs> OnChing;
        public event EventHandler<EventArgs> OnResetBuiltIn;
        public event EventHandler<ExportSimEventArgs> OnExportSim;
        public event EventHandler<PrintEventArgs> OnScriptError;
        public event EventHandler<PrintEventArgs> OnSetCurrency;
        public event EventHandler<InvestEventArgs> OnBank;

        public ProgrammerLUA(ILogger logger) : base(logger)
        {

        }
        public ProgrammerLUA()
        {
            
        }

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
        {
            try
            {
                PlaceBet NextBet = PreviousBet.CreateRetry();
                
                
                DynValue DoDiceBet = CurrentRuntime.Globals.Get("CalculateBet");
                if (DoDiceBet != null && DoDiceBet.Function!=null)
                {
                    DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
                }
                else
                {
                    
                    DoDiceBet = CurrentRuntime.Globals.Get("dobet");
                    if (DoDiceBet != null && DoDiceBet.Function!=null && NextBet is PlaceDiceBet nxt)
                    {
                        CurrentRuntime.Globals["previousbet"] = PreviousBet.TotalAmount;
                        CurrentRuntime.Globals["nextbet"] = PreviousBet.TotalAmount;
                        CurrentRuntime.Globals["win"] = PreviousBet.IsWin;
                        CurrentRuntime.Globals["currentprofit"] = ((decimal)(PreviousBet.Profit * 100000000m)) / 100000000.0m;
                        CurrentRuntime.Globals["lastBet"] = PreviousBet;
                        DynValue Result = CurrentRuntime.Call(DoDiceBet);
                        nxt.Chance = (decimal)(double)CurrentRuntime.Globals["chance"];
                        nxt.Amount = (decimal)(double)CurrentRuntime.Globals["nextbet"];
                        nxt.High = (bool)CurrentRuntime.Globals["bethigh"];
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

        //public override PlaceCrashBet CalculateNextCrashBet(CrashBet PreviousBet, bool Win)
        //{
        //    PlaceCrashBet NextBet = new PlaceCrashBet();
        //    DynValue DoDiceBet = CurrentRuntime.Globals.Get("DoCrashBet");
        //    if (DoDiceBet != null)
        //    {
        //        DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
        //    }
        //    return NextBet;
        //}

        //public override PlacePlinkoBet CalculateNextPlinkoBet(PlinkoBet PreviousBet, bool Win)
        //{
        //    PlacePlinkoBet NextBet = new PlacePlinkoBet();
        //    DynValue DoDiceBet = CurrentRuntime.Globals.Get("DoPlinkoBet");
        //    if (DoDiceBet != null)
        //    {
        //        DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
        //    }
        //    return NextBet;
        //}

        //public override PlaceRouletteBet CalculateNextRouletteBet(RouletteBet PreviousBet, bool Win)
        //{
        //    PlaceRouletteBet NextBet = new PlaceRouletteBet();
        //    DynValue DoDiceBet = CurrentRuntime.Globals.Get("DoRouletteBet");
        //    if (DoDiceBet != null)
        //    {
        //        DynValue Result = CurrentRuntime.Call(DoDiceBet, PreviousBet, Win, NextBet);
        //    }
        //    return NextBet;
        //}
       

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
            CurrentRuntime.Globals["Bank"] = (Action<decimal>)Bank;
            CurrentRuntime.Globals["Invest"] = (Action< decimal>)Invest;
            CurrentRuntime.Globals["Tip"] = (Action<string, decimal>)Tip;
            CurrentRuntime.Globals["ResetSeed"] = (Action)ResetSeed;
            CurrentRuntime.Globals["Print"] = (Action<string>)Print;
            CurrentRuntime.Globals["RunSim"] = (Action < decimal, long, bool>)RunSim;
            CurrentRuntime.Globals["ResetStats"] = (Action)ResetStats;
            CurrentRuntime.Globals["Read"] = (Func<string, int, object>)Read;
            CurrentRuntime.Globals["Readadv"] = (Func<string, int,string,string,string, object> )Readadv;
            CurrentRuntime.Globals["Alarm"] = (Action)Alarm;
            CurrentRuntime.Globals["Ching"] = (Action)Ching;
            CurrentRuntime.Globals["ResetBuiltIn"] = (Action)ResetBuiltIn;
            CurrentRuntime.Globals["ExportSim"] = (Action<string>)ExportSim;
            CurrentRuntime.Globals["Stop"] = (Action)_Stop;
            CurrentRuntime.Globals["SetCurrency"] = (Action<string>)SetCurrency;


            //legacy support
            CurrentRuntime.Globals["withdraw"] = (Action<string, decimal>)Withdraw; ;
            CurrentRuntime.Globals["invest"] = (Action<decimal>)Invest; ;
            CurrentRuntime.Globals["tip"] = (Action<string, decimal>)Tip; ;
            CurrentRuntime.Globals["stop"] = (Action)_Stop; 
            CurrentRuntime.Globals["resetseed"] = (Action)ResetSeed; ;
            CurrentRuntime.Globals["print"] = (Action<string>)Print; ;
            /*CurrentRuntime.Globals["getHistory"] = luagethistory;
            CurrentRuntime.Globals["getHistoryByDate"] = luagethistory;
            CurrentRuntime.Globals["getHistoryByQuery"] = QueryHistory;*/
            /*CurrentRuntime.Globals["runsim"] = runsim;
            CurrentRuntime.Globals["martingale"] = LuaMartingale;
            CurrentRuntime.Globals["labouchere"] = LuaLabouchere;
            CurrentRuntime.Globals["fibonacci"] = LuaFibonacci;
            CurrentRuntime.Globals["dalembert"] = LuaDAlember;
            CurrentRuntime.Globals["presetlist"] = LuaPreset;*/
            CurrentRuntime.Globals["resetstats"] = (Action)ResetStats;

            CurrentRuntime.Globals["resetprofit"]  = (Action)ResetProfit;
            CurrentRuntime.Globals["resetpartialprofit"] = (Action)ResetPartialProfit;
            /*CurrentRuntime.Globals["setvalueint"] = LuaSetValue;
            CurrentRuntime.Globals["setvaluestring"] = LuaSetValue;
            CurrentRuntime.Globals["setvaluedecimal"] = LuaSetValue;
            CurrentRuntime.Globals["setvaluebool"] = LuaSetValue;
            CurrentRuntime.Globals["getvalue"] = LuaGetValue;
            CurrentRuntime.Globals["loadstrategy"] = LuaLoadStrat;*/
            CurrentRuntime.Globals["read"] = (Func<string, int, object>)Read; ;
            CurrentRuntime.Globals["readadv"] = (Func<string, int, string, string, string, object>)Readadv; ;
            CurrentRuntime.Globals["alarm"] = (Action)Alarm; ;
            CurrentRuntime.Globals["ching"] = (Action)Ching; ;
            CurrentRuntime.Globals["resetbuiltin"] =(Action)NoAction;
            CurrentRuntime.Globals["exportsim"] = (Action<string>)ExportSim;
            CurrentRuntime.Globals["vault"] = (Action<decimal>)Bank; 

        }

        private void NoAction()
        {

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

        public override PlaceBet RunReset(Games Game)
        {
            try
            {
                DynValue DoDiceBet = CurrentRuntime.Globals.Get("Reset");
                if (DoDiceBet != null && DoDiceBet.Function!=null)
                {
                    PlaceBet NextBet = CreateEmptyPlaceBet(Game);
                    DynValue Result = CurrentRuntime.Call(DoDiceBet, NextBet, Game);
                    return NextBet;
                }
                else if (Game == Games.Dice)
                {
                    PlaceDiceBet NextBet = CreateEmptyPlaceBet(Game) as PlaceDiceBet;
                    //(decimal)CurrentRuntime.Globals["chance"];
                    NextBet.Amount = (decimal)(double)CurrentRuntime.Globals["nextbet"];
                    NextBet.Chance = (decimal)(double)CurrentRuntime.Globals["chance"];
                    NextBet.High = (bool)CurrentRuntime.Globals["bethigh"];
                    return NextBet;
                    //if (CurrentSite.Currency != (string)Lua["currency"])
                    //    CurrentSite.Currency = (string)Lua["currency"];
                    //EnableReset = (bool)Lua["enablersc"];
                    //EnableProgZigZag = (bool)Lua["enablezz"];
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

        public void SetSimulation(bool IsSimulation)
        {
            CurrentRuntime.Globals["InSimulation"] = IsSimulation;
        }

        void Withdraw(string Address, decimal Amount)
        {
            OnWithdraw?.Invoke(this, new WithdrawEventArgs { Address=Address, Amount=Amount });            
        }

        void Bank(decimal Amount)
        {
            OnBank?.Invoke(this, new InvestEventArgs { Amount = Amount });
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
        void RunSim(decimal Balance, long Bets, bool log)
        {
            OnRunSim?.Invoke(this, new RunSimEventArgs { Balance=Balance, Bets=Bets, WriteLog=log });
        }
        void ResetStats()
        {
            OnResetStats?.Invoke(this, new EventArgs());
        }
        void ResetProfit()
        {
            OnResetProfit?.Invoke(this, new EventArgs());
        }
        void ResetPartialProfit()
        {
            OnResetPartialProfit?.Invoke(this, new EventArgs());
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
                //CurrentRuntime.Globals["nextbet"] = Amount;
                //CurrentRuntime.Globals["chance"] = Chance;
                //CurrentRuntime.Globals["bethigh"] = High;
                CurrentRuntime.Globals["bets"] = this.Stats.Wins + this.Stats.Losses;
                CurrentRuntime.Globals["wins"] = this.Stats.Wins;
                CurrentRuntime.Globals["losses"] = this.Stats.Losses;
                CurrentRuntime.Globals["wagered"] = Stats.Wagered;
                CurrentRuntime.Globals["partialprofit"] = this.Stats.PartialProfit;
                

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
