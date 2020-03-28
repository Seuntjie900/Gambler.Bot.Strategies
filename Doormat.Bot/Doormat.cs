using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using DoormatCore.Storage;
using DoormatBot.Strategies;
using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static DoormatBot.Helpers.PersonalSettings;
using ErrorEventArgs = DoormatCore.Sites.ErrorEventArgs;
using DoormatBot.Helpers;

namespace DoormatBot
{
    public class Doormat
    {

        #region Internal Variables
        List<ErrorEventArgs> ActiveErrors = new List<ErrorEventArgs>();
        PwDatabase Passdb = new PwDatabase();
        

        public bool KeepassOpen
        {
            get { return Passdb.IsOpen; }            
        }


        SQLBase DBInterface { get; set; }

        string LastBetGuid = "";
        Queue<string> LastBetsGuids = new Queue<string>();

        /// <summary>
        /// Indicates that the bot is currently running placing bets
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Sets the flag to stop all betting operations.
        /// </summary>
        public bool Stop { get; private set; }

        public bool RunningSimulation { get; private set; }
        private long totalRuntime = new long();

        public long TotalRuntime
        {
            get { return totalRuntime ; }
            set { totalRuntime = value; }
        }

        public bool LoggedIn { get; set; }
        public SessionStats Stats { get; set; }
        public InternalBetSettings BetSettings { get; set; } = new InternalBetSettings();
        public PersonalSettings PersonalSettings { get; set; } = new PersonalSettings();
        Bet MostRecentBet = null;
        PlaceBet NextBext = null;
        int Retries = 0;
        //internal variables
        #endregion

        string VersionStr = "";
        public Doormat()
        {
            VersionStr = string.Format("{0}.{1}.{2}", Environment.Version.Major, Environment.Version.MajorRevision, Environment.Version.Build);
            Stats = new SessionStats();
            Running = false;
            Stop = false;
        }

        public List<SitesList> Sites = new List<SitesList>();
        public SitesList[] CompileSites()
        {
            Logger.DumpLog("Compiling Sites", 5);
            List<string> Files = new List<string>();

            Sites = new List<SitesList>();
            if (Directory.Exists("Sites"))
            {
                Logger.DumpLog("Sites dir found, searching for files",6);
                string Sites = "";
                foreach (string s in Directory.GetFiles("Sites"))
                {
                    Logger.DumpLog("Sites dir found, searching for files. Found "+s, 6);
                    string outs = File.ReadAllText(s);
                    Sites += outs;
                    Files.Add(outs);
                }

                //Compile site fies
                           
            }
            //else
            {
                Logger.DumpLog("Site dir not found, Stepping Through Types", 6);
                Type[] tps = Assembly.GetAssembly(typeof(BaseSite)).GetTypes();
                List<string> sites = new List<string>();
                foreach (Type x in tps)
                {
                    Logger.DumpLog("Stepping Through Types - " + x.Name, 6);
                    if (x.IsSubclassOf(Type.GetType("DoormatCore.Sites.BaseSite")))
                    {
                        Logger.DumpLog("Found Type - " + x.Name, 6);
                        sites.Add(x.Name);
                        string[] currenices = new string[] { "Btc" };
                        Games[] games = new Games[] { Games.Dice };
                        try
                        {
                            Logger.DumpLog("Fetching currencies for - " + x.Name, 6);

                            currenices = (Activator.CreateInstance(x) as BaseSite).Currencies;
                        }
                        catch (Exception e)
                        {
                            Logger.DumpLog(e);
                        }
                        try
                        {
                            Logger.DumpLog("Fetching currencies for - " + x.Name, 6);

                            games = (Activator.CreateInstance(x) as BaseSite).SupportedGames;
                        }
                        catch (Exception e)
                        {
                            Logger.DumpLog(e);
                        }
                        this.Sites.Add(new SitesList { Name = x.Name, Currencies = currenices, SupportedGames = games }.SetType(x));
                    }
                }
            }
            Logger.DumpLog("Populated Sites", 6);
            
            if (Sites!=null && DBInterface!=null)
            {
                Logger.DumpLog("Updating Sites Table", 6);
                foreach (SitesList x in Sites)
                {
                    Logger.DumpLog($"Fetch {x.Name} from SQL", 6);
                    Site tmp = DBInterface.FindSingle<Site>("Name=@1","",x.Name);
                    if (tmp==null)
                    {
                        Logger.DumpLog($"{x.Name} not found in sql, inserting row", 6);
                        tmp = new Site { ClassName=x.SiteType().FullName,  Name=x.Name };                        
                        tmp = DBInterface.Save<Site>(tmp);
                    }
                    else
                    {
                        Logger.DumpLog($"{x.Name} found in sql", 6);
                    }
                }
            }
            else
            {
                Logger.DumpLog("Not Updating Sites Table", 6);
            }
            return this.Sites.ToArray();            
        }

        public Dictionary<string, Type> Strategies { get; set; }
        public Dictionary<string, Type> GetStrats()
        {
            Strategies = new Dictionary<string, Type>();
            Type[] tps = Assembly.GetAssembly(typeof(BaseStrategy)).GetTypes();
            List<string> sites = new List<string>();
            Type BaseTyope = Type.GetType("DoormatCore.Strategies.BaseStrategy");
            foreach (Type x in tps)
            {
                if (x.IsSubclassOf(BaseTyope))
                {
                    Strategies.Add(x.Name,x);
                }
            }
            return Strategies;
        }

        #region Site Stuff
        private BaseSite baseSite;

        public BaseSite CurrentSite
        {
            get { return baseSite; }
            set
            {
                if (baseSite!=null)
                {
                    baseSite.Action -= BaseSite_Action;
                    baseSite.ChatReceived -= BaseSite_ChatReceived;
                    baseSite.BetFinished -= BaseSite_DiceBetFinished;
                    baseSite.Error -= BaseSite_Error;
                    baseSite.LoginFinished -= BaseSite_LoginFinished;
                    baseSite.Notify -= BaseSite_Notify;
                    baseSite.RegisterFinished -= BaseSite_RegisterFinished;
                    baseSite.StatsUpdated -= BaseSite_StatsUpdated;
                    baseSite.OnInvestFinished += BaseSite_OnInvestFinished;
                    baseSite.OnResetSeedFinished += BaseSite_OnResetSeedFinished;
                    baseSite.OnTipFinished += BaseSite_OnTipFinished;
                    baseSite.OnWithdrawalFinished += BaseSite_OnWithdrawalFinished;
                    baseSite.Disconnect();                    
                }
                baseSite = value;
                if (baseSite != null)
                {
                    baseSite.Action += BaseSite_Action;
                    baseSite.ChatReceived += BaseSite_ChatReceived;
                    baseSite.BetFinished += BaseSite_DiceBetFinished;
                    baseSite.Error += BaseSite_Error;
                    baseSite.LoginFinished += BaseSite_LoginFinished;
                    baseSite.Notify += BaseSite_Notify;
                    baseSite.RegisterFinished += BaseSite_RegisterFinished;
                    baseSite.StatsUpdated += BaseSite_StatsUpdated;
                    baseSite.OnInvestFinished += BaseSite_OnInvestFinished;
                    baseSite.OnResetSeedFinished += BaseSite_OnResetSeedFinished;
                    baseSite.OnTipFinished += BaseSite_OnTipFinished;
                    baseSite.OnWithdrawalFinished += BaseSite_OnWithdrawalFinished;
                    
                    if (!new List<Games>(baseSite.SupportedGames).Contains(CurrentGame))
                    {
                        CurrentGame = baseSite.SupportedGames[0];
                    }

                }
            }
        }

        private void BaseSite_OnWithdrawalFinished(object sender, GenericEventArgs e)
        {
            CalculateNextBet();
        }

        private void BaseSite_OnTipFinished(object sender, GenericEventArgs e)
        {
            CalculateNextBet();
        }

        private void BaseSite_OnResetSeedFinished(object sender, GenericEventArgs e)
        {
            CalculateNextBet();
        }

        private void BaseSite_OnInvestFinished(object sender, GenericEventArgs e)
        {
            CalculateNextBet();
        }

        private Games currentGame;

        public Games CurrentGame
        {
            get { return currentGame; }
            set { currentGame = value; OnGameChanged?.Invoke(this, new EventArgs()); }
        }


        public event BaseSite.dStatsUpdated OnSiteStatsUpdated;
        public event BaseSite.dAction OnSiteAction;
        public event BaseSite.dChat OnSiteChat;
        public event BaseSite.dBetFinished OnSiteBetFinished;
        public event BaseSite.dError OnSiteError;
        public event BaseSite.dLoginFinished OnSiteLoginFinished;
        public event BaseSite.dNotify OnSiteNotify;
        public event BaseSite.dRegisterFinished OnSiteRegisterFinished;
        public event EventHandler OnGameChanged;
        public event EventHandler<NotificationEventArgs> OnNotification;
        public event EventHandler<GetConstringPWEventArgs> NeedConstringPassword;
        public event EventHandler<GetConstringPWEventArgs> NeedKeepassPassword;

        private void BaseSite_StatsUpdated(object sender, StatsUpdatedEventArgs e)
        {
            OnSiteStatsUpdated?.Invoke(sender, e);
        }

        private void BaseSite_RegisterFinished(object sender, GenericEventArgs e)
        {
            OnSiteRegisterFinished?.Invoke(sender, e);
        }

        private void BaseSite_Notify(object sender, GenericEventArgs e)
        {
            OnSiteNotify?.Invoke(sender, e);
        }

        private void BaseSite_LoginFinished(object sender, LoginFinishedEventArgs e)
        {
            LoggedIn = e.Success;
            OnSiteLoginFinished?.Invoke(sender, e);
        }
        List<ErrorType> BettingErrorTypes = new List<ErrorType>(new ErrorType[] { ErrorType.BalanceTooLow, ErrorType.BetMismatch, ErrorType.InvalidBet, ErrorType.NotImplemented, ErrorType.Other, ErrorType.Unknown });
        List<ErrorType> NonBettingErrorTypes = new List<ErrorType>(new ErrorType[] { ErrorType.Withdrawal, ErrorType.Tip, ErrorType.ResetSeed });
        
        private void BaseSite_Error(object sender, ErrorEventArgs e)
        {
            ActiveErrors.Add(e);
            Strategy.OnError(e);
            if (!e.Handled)
            {
                ErrorSetting tmpSetting = PersonalSettings.GetErrorSetting(e.Type);
                if (tmpSetting!=null)
                {
                    if (tmpSetting.Action == ErrorActions.Stop)
                        StopDice(tmpSetting.Type.ToString() + " error occurred - Set to stop.");
                    
                    
                    if (BettingErrorTypes.Contains(tmpSetting.Type))
                    {
                        if (ErrorActions.Reset == tmpSetting.Action)
                        {
                            if (Running && !Stop)
                            {
                                if (Retries <= PersonalSettings.RetryAttempts)
                                {
                                    NextBext = (Strategy.RunReset());
                                    Thread.Sleep(PersonalSettings.RetryDelay);
                                    Retries++;
                                    CalculateNextBet();
                                }
                            }
                        }
                        else if (tmpSetting.Action == ErrorActions.Resume)
                        {

                        }
                        else if (ErrorActions.Retry == tmpSetting.Action)
                        {
                            if (Retries <= PersonalSettings.RetryAttempts)
                            {
                                CalculateNextBet(); Thread.Sleep(PersonalSettings.RetryDelay);
                                Retries++;
                                CalculateNextBet();
                            }
                        }
                    }
                    else
                    {
                        switch (tmpSetting.Action)
                        {
                            case ErrorActions.Reset:
                                if (Running && !Stop)
                                {
                                    if (Retries <= PersonalSettings.RetryAttempts)
                                    {
                                        NextBext = (Strategy.RunReset());
                                        Thread.Sleep(PersonalSettings.RetryDelay);
                                        Retries++;
                                        CalculateNextBet();
                                    }
                                }
                                break;
                            case ErrorActions.Resume:break;
                            case ErrorActions.Retry:
                                if (Retries <= PersonalSettings.RetryAttempts)
                                {
                                    CalculateNextBet(); Thread.Sleep(PersonalSettings.RetryDelay);
                                    Retries++;
                                    CalculateNextBet();
                                } break;
                        }
                    }
                }
            }
            OnSiteError?.Invoke(sender, e);
            try
            {
                ActiveErrors.Remove(e);
            }
            catch
            {

            }
        }

        private void BaseSite_DiceBetFinished(object sender, BetFinisedEventArgs e)
        {
            DBInterface.Save<Bet>(e.NewBet);
            MostRecentBet = e.NewBet;
            Retries = 0;
            /*
             * save bet to DB - invoke async?
             * send bet to GUI - invoke async?
             * */
            bool win = e.NewBet.GetWin(CurrentSite); 
            string Response = "";
            bool Reset = false;
            Stats.UpdateStats(e.NewBet, win);
            if (Strategy is ProgrammerMode)
            {
                (Strategy as ProgrammerMode).UpdateSessionStats(CopyHelper.CreateCopy<SessionStats>(Stats));
                (Strategy as ProgrammerMode).UpdateSiteStats(CopyHelper.CreateCopy<SiteStats>(CurrentSite.Stats));
                (Strategy as ProgrammerMode).UpdateSite(CopyHelper.CreateCopy<SiteDetails>(CurrentSite.SiteDetails));
            }
            OnSiteBetFinished?.Invoke(sender, e );


            if (e.NewBet.Guid!=LastBetGuid || LastBetsGuids.Contains(e.NewBet.Guid))
            {
                StopDice("Last bet did not match the latest bet placed.");
                //stop
                return;
            }
            else
            {
                LastBetsGuids.Enqueue(e.NewBet.Guid);
                if (LastBetsGuids.Count > 10)
                    LastBetsGuids.Dequeue();
            }


            


            foreach (Trigger x in PersonalSettings.Notifications)
            {
                if (x.Enabled)
                {
                    if (x.CheckNotification(Stats))
                    {
                        switch (x.Action)
                        {
                            case TriggerAction.Alarm:
                            case TriggerAction.Chime:
                            case TriggerAction.Popup: OnNotification?.Invoke(this, new NotificationEventArgs { NotificationTrigger = x }); break;
                            case TriggerAction.Email: throw new NotImplementedException("Supporting infrastructure for this still needs to be built.");                                
                        }
                    }
                }
            }
            NextBext = null;            
            
            foreach (Trigger x in BetSettings.Triggers)
            {
                if (x.Enabled)
                {
                    if (x.CheckNotification(Stats))
                    {
                        switch (x.Action)
                        {
                            case TriggerAction.Bank:throw new NotImplementedException(); break;
                            case TriggerAction.Invest: CurrentSite.Invest(x.GetValue(Stats)); break;
                            case TriggerAction.Reset: NextBext = Strategy.RunReset(); Reset = true; break;
                            case TriggerAction.ResetSeed: if (CurrentSite.CanChangeSeed) CurrentSite.ResetSeed(CurrentSite.GenerateNewClientSeed()); break;
                            case TriggerAction.Stop: StopDice("Stop trigger fired. Will show more detail about the trigger here later."); break;
                            //case TriggerAction.Switch: Strategy.High = !Strategy.High; if (NewBetObject != null)NewBetObject.High = !e.NewBet.High;  break;
                            case TriggerAction.Tip: CurrentSite.SendTip(x.Destination, x.GetValue(Stats)); break;
                            case TriggerAction.Withdraw: CurrentSite.Withdraw(x.Destination, x.GetValue(Stats)); break;

                        }
                    }
                }
            }
            CalculateNextBet();
        }

        void CalculateNextBet()
        {
            if (CurrentSite.ActiveActions.Count > 0 || ActiveErrors.Count > 0)
                return;
            if (Strategy is ProgrammerMode)
            {
                (Strategy as ProgrammerMode).UpdateSessionStats(CopyHelper.CreateCopy<SessionStats>(Stats));
                (Strategy as ProgrammerMode).UpdateSiteStats(CopyHelper.CreateCopy<SiteStats>(CurrentSite.Stats));
                (Strategy as ProgrammerMode).UpdateSite(CopyHelper.CreateCopy<SiteDetails>(CurrentSite.SiteDetails));
            }
            bool win = MostRecentBet.GetWin(CurrentSite);
            if (NextBext ==null)
                NextBext = Strategy.CalculateNextBet(MostRecentBet, win);
            if (Running && !Stop)
            {
                while (CurrentSite.TimeToBet(NextBext) > 0)
                {
                    int TimeToBet = CurrentSite.TimeToBet(NextBext);
                    if (TimeToBet < 0)
                        TimeToBet = (10);
                    Thread.Sleep(TimeToBet);
                }
                if (Running && !Stop)
                    PlaceBet(NextBext);
            }
        }

        private void BaseSite_ChatReceived(object sender, GenericEventArgs e)
        {
            OnSiteChat?.Invoke(sender, e);
        }

        private void BaseSite_Action(object sender, GenericEventArgs e)
        {
            OnSiteAction?.Invoke(sender, e);
        }

        public void Login(BaseSite.LoginParamValue[] LoginParams)
        {
            if (CurrentSite==null)
            {
                throw new Exception("Cannot login without a site. Assign a value to CurrentSite, then log in.");
            }
            CurrentSite.LogIn(LoginParams);
        }

        

        //Site Stuff
        #endregion

        private Strategies.BaseStrategy strategy;

        public Strategies.BaseStrategy Strategy
        {
            get { return strategy; }
            set
            {
                if (strategy != null)
                {
                    strategy.NeedBalance -= Strategy_NeedBalance;
                    strategy.Stop -= Strategy_Stop;
                    strategy.OnNeedStats -= Strategy_OnNeedStats;
                    if (strategy is ProgrammerMode)
                    {
                        (Strategy as ProgrammerMode).OnAlarm -= Doormat_OnAlarm;
                        (Strategy as ProgrammerMode).OnChing -= Doormat_OnChing;
                        (Strategy as ProgrammerMode).OnExportSim -= Doormat_OnExportSim;
                        (Strategy as ProgrammerMode).OnInvest -= Doormat_OnInvest;
                        (Strategy as ProgrammerMode).OnPrint -= Doormat_OnPrint;
                        (Strategy as ProgrammerMode).OnRead -= Doormat_OnRead;
                        (Strategy as ProgrammerMode).OnReadAdv -= Doormat_OnReadAdv;
                        //(Strategy as ProgrammerMode).OnResetBuiltIn -= Doormat_OnResetBuiltIn;
                        (Strategy as ProgrammerMode).OnResetSeed -= Doormat_OnResetSeed;
                        (Strategy as ProgrammerMode).OnResetStats -= Doormat_OnResetStats;
                        (Strategy as ProgrammerMode).OnRunSim -= Doormat_OnRunSim;
                        (Strategy as ProgrammerMode).OnStop -= Doormat_OnStop;
                        (Strategy as ProgrammerMode).OnTip -= Doormat_OnTip;
                        (Strategy as ProgrammerMode).OnWithdraw -= Doormat_OnWithdraw;

                    }
                }
                strategy = value;
                if (strategy != null)
                {
                    strategy.NeedBalance += Strategy_NeedBalance;
                    strategy.Stop += Strategy_Stop;
                    strategy.OnNeedStats += Strategy_OnNeedStats;
                    if (strategy is ProgrammerMode)
                    {
                        (strategy as ProgrammerMode).CreateRuntime();
                        (Strategy as ProgrammerMode).OnAlarm += Doormat_OnAlarm;
                        (Strategy as ProgrammerMode).OnChing += Doormat_OnChing;
                        (Strategy as ProgrammerMode).OnExportSim += Doormat_OnExportSim;
                        (Strategy as ProgrammerMode).OnInvest += Doormat_OnInvest;
                        (Strategy as ProgrammerMode).OnPrint += Doormat_OnPrint;
                        (Strategy as ProgrammerMode).OnRead += Doormat_OnRead;
                        (Strategy as ProgrammerMode).OnReadAdv += Doormat_OnReadAdv;
                        //(Strategy as ProgrammerMode).OnResetBuiltIn += Doormat_OnResetBuiltIn;
                        (Strategy as ProgrammerMode).OnResetSeed += Doormat_OnResetSeed;
                        (Strategy as ProgrammerMode).OnResetStats += Doormat_OnResetStats;
                        (Strategy as ProgrammerMode).OnRunSim += Doormat_OnRunSim;
                        (Strategy as ProgrammerMode).OnStop += Doormat_OnStop;
                        (Strategy as ProgrammerMode).OnTip += Doormat_OnTip;
                        (Strategy as ProgrammerMode).OnWithdraw += Doormat_OnWithdraw;                        
                    }
                }
                
            }
        }

        private void Doormat_OnWithdraw(object sender, WithdrawEventArgs e)
        {
            if (CurrentSite.AutoWithdraw)
                CurrentSite.Withdraw(e.Address, e.Amount);
        }

        private void Doormat_OnTip(object sender, TipEventArgs e)
        {
            if (CurrentSite.CanTip)
                CurrentSite.SendTip(e.Receiver, e.Amount);
        }

        private void Doormat_OnStop(object sender, EventArgs e)
        {
            StopDice("Programmer mode stop signal received.");
        }

        private void Doormat_OnRunSim(object sender, RunSimEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Doormat_OnResetStats(object sender, EventArgs e)
        {
            ResetStats();
        }

        private void Doormat_OnResetSeed(object sender, EventArgs e)
        {
            if (CurrentSite.CanChangeSeed)
                CurrentSite.ResetSeed(CurrentSite.R.Next(0, int.MaxValue).ToString());
        }

        private void Doormat_OnResetBuiltIn(object sender, EventArgs e)
        {
            Strategy.RunReset();
        }

        private void Doormat_OnReadAdv(object sender, ReadEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Doormat_OnRead(object sender, ReadEventArgs e)

        {
            throw new NotImplementedException();
        }

        private void Doormat_OnPrint(object sender, PrintEventArgs e)
        {
            //send print to UI
            throw new NotImplementedException();
        }

        private void Doormat_OnInvest(object sender, InvestEventArgs e)
        {
            if (CurrentSite.AutoInvest)
                CurrentSite.Invest(e.Amount);
        }

        private void Doormat_OnExportSim(object sender, ExportSimEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Doormat_OnChing(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Doormat_OnAlarm(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private SessionStats Strategy_OnNeedStats(object sender, EventArgs e)
        {
            return CopyHelper.CreateCopy<SessionStats>(Stats);
        }

        private void Strategy_Stop(object sender, Strategies.StopEventArgs e)
        {
            StopDice(e.Reason);
        }

        private decimal Strategy_NeedBalance()
        {
            if (CurrentSite == null)
                return 0;
            else if (CurrentSite.Stats == null)
                return 0;
            else
                return CurrentSite.Stats.Balance;
        }
       
        public void StartDice()
        {
            CurrentSite.ActiveActions.Clear();
            ActiveErrors.Clear();
            if (Running)
                throw new Exception("Cannot start bot while it's running");
            if (RunningSimulation)
                throw new Exception("Cannot start bot while it's running a simulation");
            if (!Running && !RunningSimulation)
            {
                if (Strategy is ProgrammerMode)
                {
                    (Strategy as ProgrammerMode).LoadScript();
                    (Strategy as ProgrammerMode).UpdateSessionStats(CopyHelper.CreateCopy<SessionStats>(Stats));
                    (Strategy as ProgrammerMode).UpdateSiteStats(CopyHelper.CreateCopy<SiteStats>(CurrentSite.Stats));
                    (Strategy as ProgrammerMode).UpdateSite(CopyHelper.CreateCopy<SiteDetails>(CurrentSite.SiteDetails));
                }
                Running = true;
                Stats.StartTime = DateTime.Now;
                PlaceBet(strategy.RunReset() /*new Helpers.PlaceDiceBet((decimal)Strategy.Amount, strategy.High, (decimal)Strategy.Chance)*/);
            }
            /*
             * if not running
             * and not running simulator
             * get initial bet values (chance, high, amount)
             * mark bot as running
             * reset run variables
             * continue session variables
             * run dicebet thread to place initial bet
             */
        }

        public void StopDice(string Reason)
        {
            Running = false;
            Stats.EndTime = DateTime.Now;
            Stats.RunningTime += (long)(Stats.EndTime - Stats.EndTime).TotalMilliseconds;
            Stats= DBInterface.Save<SessionStats>(Stats);
            //TotalRuntime +=Stats.EndTime - Stats.StartTime;
            Logger.DumpLog(Reason, 3);
        }

        public void ResetStats()
        {
            if (Running)
            {
                Stats.EndTime = DateTime.Now;
                Stats.RunningTime += (long)(Stats.EndTime - Stats.EndTime).TotalMilliseconds;
            }
            TotalRuntime += Stats.RunningTime;
            Stats = this.DBInterface.Save<SessionStats>(Stats);

            Stats = new SessionStats();
        }

        public void PlaceBet(PlaceBet Bet)
        {
            Bet.GUID = Guid.NewGuid().ToString();
            LastBetGuid = Bet.GUID;
            CurrentSite.PlaceBet(Bet);
        }

        void DiceBetThread(object Bet)
        {
            PlaceDiceBet tmpBet = Bet as PlaceDiceBet;
            CurrentSite.PlaceBet(tmpBet);
        }

        public class ExportBetSettings
        {
            public InternalBetSettings BetSettings { get; set; }
            public Strategies.DAlembert dAlembert { get; set; }
            public Strategies.Fibonacci Fibonacci { get; set; }
            public Strategies.Labouchere Labouchere { get; set; }
            public Strategies.PresetList PresetList { get; set; }
            public Strategies.Martingale Martingale { get; set; }
            public Strategies.ProgrammerCS ProgrammerCS { get; set; }
            public Strategies.ProgrammerJS ProgrammerJS { get; set; }
            public Strategies.ProgrammerLUA ProgrammerLUA { get; set; }
            //public Strategies.ProgrammerPython ProgrammerPython { get; set; }
            //public Strategies.Dice.Programmer Programmer { get; set; }
            public Strategies.BaseStrategy GetStrat()
            {
                if (dAlembert != null)
                    return dAlembert;
                if (PresetList != null)
                    return PresetList;
                if (Labouchere != null)
                    return Labouchere;
                if (Fibonacci != null)
                    return Fibonacci;
                if (Martingale != null)
                    return Martingale;
                if (ProgrammerCS != null)
                    return ProgrammerCS;
                if (ProgrammerJS != null)
                    return ProgrammerJS;
                if (ProgrammerLUA != null)
                    return ProgrammerLUA;
               /* if (ProgrammerPython != null)
                    return ProgrammerPython;*/

                else
                    return new Strategies.Martingale();
            }
            public void SetStrategy(Strategies.BaseStrategy Strat)
            {
                dAlembert = null;
                Fibonacci = null;
                Labouchere = null;
                PresetList = null;
                Martingale = null;
                ProgrammerCS = null;
                ProgrammerJS = null;
                ProgrammerLUA = null;
                //ProgrammerPython = null;
                if (Strat is Strategies.DAlembert)
                {
                    dAlembert = Strat as Strategies.DAlembert;                    
                }
                if (Strat is Strategies.Fibonacci)
                {                    
                    Fibonacci = Strat as Strategies.Fibonacci;                    
                }
                if (Strat is Strategies.Labouchere)
                {                    
                    Labouchere = Strat as Strategies.Labouchere;                   
                }
                if (Strat is Strategies.PresetList)
                {                   
                    PresetList = Strat as Strategies.PresetList;                    
                }
                if (Strat is Strategies.Martingale)                {
                    
                    Martingale = Strat as Strategies.Martingale;
                }
                if (Strat is Strategies.ProgrammerCS)
                {
                    ProgrammerCS = Strat as Strategies.ProgrammerCS;
                }
                if (Strat is Strategies.ProgrammerJS)
                {
                    ProgrammerJS = Strat as Strategies.ProgrammerJS;
                }
                if (Strat is Strategies.ProgrammerLUA)
                {
                    ProgrammerLUA = Strat as Strategies.ProgrammerLUA;
                }
              /* if (Strat is Strategies.ProgrammerPython)
                {
                    ProgrammerPython = Strat as Strategies.ProgrammerPython;
                }*/
            }
        }

        public void SavePersonalSettings(string FileLocation)
        {
            string Settings = DoormatCore.Helpers.json.JsonSerializer(PersonalSettings);
            using (StreamWriter sw = new StreamWriter(FileLocation, false))
            {
                sw.Write(Settings);
            }
            //incl proxy settings without password - prompt password on startup.

        }
        
        public void SaveBetSettings(string FileLocation)
        {
            ExportBetSettings tmp = new ExportBetSettings()
            {
                BetSettings = this.BetSettings
            };
            tmp.SetStrategy(Strategy);
            string Settings = DoormatCore.Helpers.json.JsonSerializer(tmp);
            using (StreamWriter sw = new StreamWriter(FileLocation, false)) 
            {
                sw.Write(Settings);
            }
            
        }

        public void LoadPersonalSettings(string FileLocation)
        {
            string Settings = "";
            using (StreamReader sr = new StreamReader(FileLocation))
            {
                Settings = sr.ReadToEnd();
            }
            Logger.DumpLog("Loaded Personal Settings File", 5);
            PersonalSettings tmp = json.JsonDeserialize<PersonalSettings>(Settings);
            Logger.DumpLog("Parsed Personal Settings File", 5);
            this.PersonalSettings = tmp;
            string pw = "";

            if (tmp.EncryptConstring)
            {
                GetConstringPWEventArgs tmpArgs = new GetConstringPWEventArgs();
                NeedConstringPassword?.Invoke(this,tmpArgs);
                pw = tmpArgs.Password;
            }
            if (!string.IsNullOrWhiteSpace(tmp.KeepassDatabase))
            {
                try
                {
                    GetConstringPWEventArgs tmpArgs = new GetConstringPWEventArgs();
                    NeedKeepassPassword?.Invoke(this, tmpArgs);
                    var ioConnInfo = new IOConnectionInfo { Path = tmp.KeepassDatabase };
                    var compKey = new CompositeKey();
                    compKey.AddUserKey(new KcpPassword(tmpArgs.Password));
                    Passdb.Open(ioConnInfo, compKey, null);
                }
                catch (Exception exc)
                {
                    OnSiteNotify?.Invoke(this, new GenericEventArgs { Message="Failed to open KeePass Database." });
                }
            }
            try
            {
                Logger.DumpLog("Attempting DB Interface Creation", 6);
                //get a list of loaded assemblies
                //get a list of classes that inherit persistentbase
                List<Type> PersistentTypes = new List<Type>();

                DBInterface = SQLBase.OpenConnection(PersonalSettings.GetConnectionString(pw), PersonalSettings.Provider, PersistentTypes);
                Logger.DumpLog("DB Interface Created", 5);
            }
            catch (Exception e)
            {
                Logger.DumpLog(e); 
                DBInterface = null;
            }
        }

        public void LoadBetSettings(string FileLocation)
        {
            string Settings = "";
            using (StreamReader sr = new StreamReader(FileLocation))
            {
                Settings = sr.ReadToEnd();
            }
            ExportBetSettings tmp = json.JsonDeserialize<ExportBetSettings>(Settings);
            if (tmp.BetSettings!=null)
                this.BetSettings = tmp.BetSettings;
            this.Strategy = tmp.GetStrat();
        }

        #region Accounts
        public KPHelper[] GetAccounts()
        {
            var Entries = Passdb.RootGroup.GetEntries(true);
            List<KPHelper> tmpHelpers = new List<KPHelper>();
            int i = 0;
            foreach (var x in Entries)
            {
                tmpHelpers.Add(new KPHelper {
                    Index = i++,
                    Title = x.Strings.ReadSafe("Title"),
                    Username = x.Strings.ReadSafe("Username"),
                    URL = x.Strings.ReadSafe("URL"),
                    Id = x.Uuid.UuidBytes
                });
            }
            //Passdb.RootGroup.FindEntry(new PwUuid(tmpHelpers[0].Id),true);
            return tmpHelpers.ToArray();
        }

        public string GetPw(KPHelper Helper, out string Note)
        {
            PwEntry pwEntry = Passdb.RootGroup.FindEntry(new PwUuid(Helper.Id), true);
            Note = pwEntry.Strings.ReadSafe("Note");
            return pwEntry.Strings.ReadSafe("Password");
        }
        #endregion

    }
}
