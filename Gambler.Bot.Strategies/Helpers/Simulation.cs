using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.Common.Events;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Limbo;

namespace Gambler.Bot.Strategies.Helpers
{
    public class Simulation
    {
        private readonly ILogger _Logger;
        public event EventHandler OnSimulationWriting;
        public event EventHandler OnSimulationComplete;
        public event EventHandler<BetFinisedEventArgs> OnBetSimulated;

        public string serverseedhash { get; set; }
        public string serverseed { get; set; }
        public string clientseed { get; set; }
        public List<string> bets = new List<string>();
        public IProvablyFair LuckyGenerator { get; set; }
        public SiteDetails Site { get; set; }
        public BaseStrategy DiceStrategy { get; set; }
        public SessionStats Stats { get; set; }
        public InternalBetSettings BetSettings { get; set; }
        SiteStats SiteStats = null;
        public decimal Balance { get; set; }
        
        public long Bets { get; set; }
        public long TotalBetsPlaced { get; private set; } = 0;
        private long BetsWithSeed = 0;
        bool Stop = false;
        bool Running = false;
        string TmpFileName = "";
        public decimal Profit { get; set; } = 0;
        bool log = true;
        public Games CurrentGame { get; set; } = Games.Dice;

        public Simulation(ILogger logger)
        {
            _Logger = logger;
        }

        public void Initialize(decimal balance, 
            long bets, 
            IProvablyFair luckyGenerator, 
            SiteDetails site,
            BaseStrategy _DiceStrategy, 
            InternalBetSettings OtherSettings, 
            string TempStorage,
            bool Log)
        {
            this.Balance = balance;
            this.Bets = bets;
            this.Site = site;
            this.BetSettings = OtherSettings;
            this.LuckyGenerator = luckyGenerator;
            ///copy strategy
            this.DiceStrategy = CopyHelper.CreateCopy(_DiceStrategy.GetType(), _DiceStrategy) as BaseStrategy;
            if (this.DiceStrategy is IProgrammerMode progmode)
            {
                (this.DiceStrategy as IProgrammerMode).CreateRuntime();
                
            }
            if (DiceStrategy != null)
            {
                this.DiceStrategy.NeedBalance += DiceStrategy_NeedBalance;
                this.DiceStrategy.OnNeedStats += DiceStrategy_OnNeedStats;
                this.DiceStrategy.Stop += DiceStrategy_Stop;
                
            }
            this.log = Log;
            if (log)
            {
                string siminfo = "Dice Bot Simulation,,Starting Balance,Amount of bets, Server seed,,,Client Seed";
                string result = ",," + balance + "," + bets + "," + serverseed + ",,," + clientseed;
                string columns = "Bet Number,LuckyNumber,Chance,Roll,Result,Wagered,Profit,Balance,Total Profit";
                this.bets.Add(siminfo);
                this.bets.Add(result);
                this.bets.Add("");
                this.bets.Add(columns);
            }
            TmpFileName = TempStorage + (LuckyGenerator?.Random.Next()??0)+".csv."+ Process.GetCurrentProcess().Id;
        }

        public void Save(string NewFile)
        {
            if (!Directory.Exists(Path.GetDirectoryName(NewFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(NewFile));
            File.Move(TmpFileName, NewFile);
           
        }

        public void Start(Games startingGame)
        {
            this.CurrentGame = startingGame;
            this.Stats = new SessionStats(true);
            this.SiteStats = new SiteStats();
            SiteStats.Balance = Balance;
            Running = true;
            Stop = false;
            if (DiceStrategy is IProgrammerMode prog)
            {
                prog.SetSimulation(true);
                prog.LoadScript();                
            }
            new Thread(new ThreadStart(SimulationThread)).Start();
        }

        public void StopSim()
        {
            Stop = true;
        }

        private void SimulationThread()
        {
            try
            {
                Bet NewBet = SimulatedBet(DiceStrategy.RunReset(CurrentGame));
                this.Balance += (decimal)NewBet.Profit;
                Profit += (decimal)NewBet.Profit;
                while (TotalBetsPlaced < Bets && !Stop && Running)
                {
                    if (log)
                    {
                        bets.Add(NewBet.ToCSV(Site.GameSettings[NewBet.Game.ToString()], TotalBetsPlaced, Balance));
                    }

                    if (TotalBetsPlaced % 10000 == 0)
                    {
                        OnSimulationWriting?.Invoke(this, new EventArgs());
                        if (log)
                        {
                            using (StreamWriter sw = File.AppendText(TmpFileName))
                            {
                                foreach (string tmpbet in bets)
                                {
                                    sw.WriteLine(tmpbet);
                                }
                            }
                            bets.Clear();
                        }
                    }

                    TotalBetsPlaced++;
                    BetsWithSeed++;
                    bool Reset = false;
                    PlaceBet NewBetObject = null;
                    bool win = NewBet.IsWin;
                    string Response = "";
                    if (BetSettings.CheckResetPreStats(NewBet, win, Stats, SiteStats)) 
                    {
                        Reset = true;
                        NewBetObject = DiceStrategy.RunReset(CurrentGame) as PlaceDiceBet;
                    }
                    if (BetSettings.CheckStopPreStats(NewBet, win, Stats, out Response, SiteStats))
                    {
                        this.Stop = (true);
                    }
                    Stats.UpdateStats(NewBet, win);
                    if (DiceStrategy is IProgrammerMode)
                    {
                        (DiceStrategy as IProgrammerMode).UpdateSessionStats(CopyHelper.CreateCopy<SessionStats>(Stats));
                        (DiceStrategy as IProgrammerMode).UpdateSiteStats(CopyHelper.CreateCopy<SiteStats>(SiteStats));
                        (DiceStrategy as IProgrammerMode).UpdateSite(Site,"" );
                    }
                    if (BetSettings.CheckResetPostStats(NewBet, win, Stats, SiteStats))
                    {
                        Reset = true;
                        NewBetObject = DiceStrategy.RunReset(CurrentGame) as PlaceDiceBet;
                    }
                    if (BetSettings.CheckStopPOstStats(NewBet, win, Stats, out Response, SiteStats))
                    {
                        Stop = true;
                    }
                    decimal withdrawamount = 0;
                    string address = "";
                    if (BetSettings.CheckWithdraw(NewBet, win, Stats, out withdrawamount, SiteStats, out address))
                    {
                        this.Balance -= withdrawamount;
                    }
                    if (BetSettings.CheckBank(NewBet, win, Stats, out withdrawamount, SiteStats))
                    {
                        this.Balance -= withdrawamount;
                    }
                    if (BetSettings.CheckTips(NewBet, win, Stats, out withdrawamount, SiteStats, out address))
                    {
                        this.Balance -= withdrawamount;
                    }
                    bool NewHigh = false;
                    if (BetSettings.CheckResetSeed(NewBet, win, Stats, SiteStats))
                    {
                        GenerateSeeds();
                    }
                    if (BetSettings.CheckHighLow(NewBet, win, Stats, out NewHigh, SiteStats))
                    {
                        (DiceStrategy as iDiceStrategy).High = NewHigh;
                    }
                    if (!Reset)
                        NewBetObject = DiceStrategy.CalculateNextBet(NewBet, win) as PlaceBet;
                    if (Running && !Stop && TotalBetsPlaced <= Bets)
                    {
                        if (this.Balance <(decimal)NewBetObject.Amount)
                        {
                            break;
                        }
                        NewBet = SimulatedBet(NewBetObject);
                        this.Balance += (decimal)NewBet.Profit;
                        Profit += (decimal)NewBet.Profit;
                        //save to file
                    }
                }
                
                using (StreamWriter sw = File.AppendText(TmpFileName))
                {
                    foreach (string tmpbet in bets)
                    {
                        sw.WriteLine(tmpbet);
                    }
                }
                bets.Clear();
                OnSimulationComplete?.Invoke(this, new EventArgs());
            }
            catch (Exception e)
            {
                Running = false;
                OnSimulationComplete?.Invoke(this, new EventArgs());
                _Logger?.LogError(e.ToString());
            }
        }

        

        public void GenerateSeeds()
        {
            clientseed = LuckyGenerator.GenerateNewClientSeed();
            //new server seed
            //new client seed
            string serverseed = "";
            string Alphabet = "1234567890QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm";
            while (serverseed.Length<64)
            {
                serverseed += Alphabet[LuckyGenerator.Random.Next(0, Alphabet.Length)];
            }
            this.serverseed = serverseed;
            //new server seed hash
            serverseedhash = LuckyGenerator.GetHash(serverseed);
            BetsWithSeed = 0;
        }


        private Bet SimulatedBet(PlaceBet NewBet)
        {
            //get RNG result from site
            IGameResult Lucky = null;
            if (!Site.NonceBased)
            {
                GenerateSeeds();
            }
            else if (string.IsNullOrEmpty(serverseed) || string.IsNullOrEmpty(clientseed))
            {
                GenerateSeeds();
            }
                
            
            Lucky=LuckyGenerator.GetLucky(serverseed, clientseed, (int)BetsWithSeed, NewBet.Game);
            Bet betresult = null;
            if (NewBet is PlaceDiceBet pd && Lucky is DiceResult diceResult)
            { 
                betresult = new DiceBet
                {
                    TotalAmount = pd.Amount,
                    Chance = pd.Chance,
                    ClientSeed = clientseed,
                    Currency = "simulation",
                    DateValue = DateTime.Now,
                    Guid = null,
                    High = pd.High,
                    Nonce = BetsWithSeed,
                    Roll = diceResult.Roll, // to do fix this but the whole simulation thing needs fixing
                    ServerHash = serverseedhash,
                    ServerSeed = serverseed
                };
                betresult.IsWin = betresult.GetWin(Site.GameSettings[NewBet.Game.ToString()]);
                betresult.Profit = betresult.IsWin ? ((((100.0m - (Site.GameSettings[NewBet.Game.ToString()] as DiceConfig).Edge) / pd.Chance) * NewBet.Amount) - NewBet.Amount) : -NewBet.Amount;
            }
            else if (NewBet is PlaceTwistBet tb && Lucky is TwistResult tr)
            {
                betresult = new TwistBet
                {
                    TotalAmount = tb.Amount,
                    Chance = tb.Chance,
                    ClientSeed = clientseed,
                    Currency = "simulation",
                    DateValue = DateTime.Now,
                    Guid = null,
                    High = tb.High,
                    Nonce = BetsWithSeed,
                    Roll = tr.Roll, // to do fix this but the whole simulation thing needs fixing
                    ServerHash = serverseedhash,
                    ServerSeed = serverseed
                };
                betresult.IsWin = betresult.GetWin(Site.GameSettings[NewBet.Game.ToString()]);
                betresult.Profit = betresult.IsWin ? ((((100.0m - (Site.GameSettings[NewBet.Game.ToString()] as TwistConfig).Edge) / tb.Chance) * NewBet.Amount) - NewBet.Amount) : -NewBet.Amount;
            }
            else if (NewBet is PlaceLimboBet lb && Lucky is LimboResult lr)
            {
                betresult = new LimboBet
                {
                    TotalAmount = lb.Amount,
                    Chance = lb.Chance,
                    ClientSeed = clientseed,
                    Currency = "simulation",
                    DateValue = DateTime.Now,
                    Guid = null,
                    Nonce = BetsWithSeed,
                    Result = lr.Result, // to do fix this but the whole simulation thing needs fixing
                    ServerHash = serverseedhash,
                    ServerSeed = serverseed
                };
                betresult.IsWin = betresult.GetWin(Site.GameSettings[NewBet.Game.ToString()]);
                betresult.Profit = betresult.IsWin ? (((((Site.GameSettings[NewBet.Game.ToString()] as LimboConfig).Edge) / lb.Chance) * NewBet.Amount) - NewBet.Amount) : -NewBet.Amount;
            }

            OnBetSimulated?.Invoke(this, new BetFinisedEventArgs(betresult));
            return betresult;
        }

        private void DiceStrategy_Stop(object sender, StopEventArgs e)
        {
            Stop = true;
        }

        private SessionStats DiceStrategy_OnNeedStats(object sender, EventArgs e)
        {
            return Stats;
        }

        private decimal DiceStrategy_NeedBalance()
        {
            return (decimal)Balance;
        }

        public void MoveLog(string NewLocation)
        {
            File.Move(TmpFileName, NewLocation);
        }
        public Stream GetStream()
        {
            return File.OpenRead(TmpFileName);
        }
        public void DeleteLog()
        {
            File.Delete(TmpFileName);
        }
    }
}
