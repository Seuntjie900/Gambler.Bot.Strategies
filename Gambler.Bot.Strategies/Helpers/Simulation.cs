﻿using Gambler.Bot.Strategies.Strategies.Abstractions;
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
            if (this.DiceStrategy is IProgrammerMode)
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

        public void Start()
        {
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
                DiceBet NewBet = SimulatedBet(DiceStrategy.RunReset(Games.Dice) as PlaceDiceBet);
                this.Balance += (decimal)NewBet.Profit;
                Profit += (decimal)NewBet.Profit;
                while (TotalBetsPlaced < Bets && !Stop && Running)
                {
                    if (log)
                    {
                        bets.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}"
                        , TotalBetsPlaced, NewBet.Roll, NewBet.Chance, (NewBet.High ? ">" : "<"), NewBet.GetWin(Site.maxroll) ? "win" : "lose", NewBet.TotalAmount, NewBet.Profit, this.Balance, Profit));
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
                    PlaceDiceBet NewBetObject = null;
                    bool win = NewBet.GetWin(Site.maxroll);
                    string Response = "";
                    if (BetSettings.CheckResetPreStats(NewBet, NewBet.GetWin(Site.maxroll), Stats, SiteStats)) 
                    {
                        Reset = true;
                        NewBetObject = DiceStrategy.RunReset(Games.Dice) as PlaceDiceBet;
                    }
                    if (BetSettings.CheckStopPreStats(NewBet, NewBet.GetWin(Site.maxroll), Stats, out Response, SiteStats))
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
                    if (BetSettings.CheckResetPostStats(NewBet, NewBet.GetWin(Site.maxroll), Stats, SiteStats))
                    {
                        Reset = true;
                        NewBetObject = DiceStrategy.RunReset(Games.Dice) as PlaceDiceBet;
                    }
                    if (BetSettings.CheckStopPOstStats(NewBet, NewBet.GetWin(Site.maxroll), Stats, out Response, SiteStats))
                    {
                        Stop = true;
                    }
                    decimal withdrawamount = 0;
                    string address = "";
                    if (BetSettings.CheckWithdraw(NewBet, NewBet.GetWin(Site.maxroll), Stats, out withdrawamount, SiteStats, out address))
                    {
                        this.Balance -= withdrawamount;
                    }
                    if (BetSettings.CheckBank(NewBet, NewBet.GetWin(Site.maxroll), Stats, out withdrawamount, SiteStats))
                    {
                        this.Balance -= withdrawamount;
                    }
                    if (BetSettings.CheckTips(NewBet, NewBet.GetWin(Site.maxroll), Stats, out withdrawamount, SiteStats, out address))
                    {
                        this.Balance -= withdrawamount;
                    }
                    bool NewHigh = false;
                    if (BetSettings.CheckResetSeed(NewBet, NewBet.GetWin(Site.maxroll), Stats, SiteStats))
                    {
                        GenerateSeeds();
                    }
                    if (BetSettings.CheckHighLow(NewBet, NewBet.GetWin(Site.maxroll), Stats, out NewHigh, SiteStats))
                    {
                        (DiceStrategy as iDiceStrategy).High = NewHigh;
                    }
                    if (!Reset)
                        NewBetObject = DiceStrategy.CalculateNextBet(NewBet, win) as PlaceDiceBet;
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


        private DiceBet SimulatedBet(PlaceDiceBet NewBet)
        {
            //get RNG result from site
            decimal Lucky = 0;
            if (!Site.NonceBased)
            {
                GenerateSeeds();
            }
            else if (string.IsNullOrEmpty(serverseed) || string.IsNullOrEmpty(clientseed))
            {
                GenerateSeeds();
            }
                
            
            Lucky=LuckyGenerator.GetLucky(serverseed, clientseed, (int)BetsWithSeed);
            
            DiceBet betresult = new DiceBet {
                TotalAmount = NewBet.Amount,
                Chance = NewBet.Chance,
                ClientSeed = clientseed,
                Currency = "simulation",
                DateValue = DateTime.Now,
                Guid = null,
                High = NewBet.High,
                Nonce = BetsWithSeed,
                Roll = Lucky,
                ServerHash = serverseedhash,
                ServerSeed = serverseed
            };
            betresult.Profit = betresult.GetWin(Site.maxroll) ?  ((((100.0m - Site.edge) / NewBet.Chance) * NewBet.Amount)-NewBet.Amount): -NewBet.Amount;
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
