using DoormatCore.Games;
using DoormatCore.Sites;
using DoormatBot.Strategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DoormatCore.Helpers;

namespace DoormatBot.Helpers
{
    public class Simulation
    {
        public event EventHandler OnSimulationWriting;
        public event EventHandler OnSimulationComplete;

        public string serverseedhash { get; set; }
        public string serverseed { get; set; }
        public string clientseed { get; set; }
        public List<string> bets = new List<string>();
        public DoormatCore.Sites.BaseSite Site { get; set; }
        public Strategies.BaseStrategy DiceStrategy { get; set; }
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
        public Simulation(decimal balance, long bets, DoormatCore.Sites.BaseSite Site, Strategies.BaseStrategy DiceStrategy, InternalBetSettings OtherSettings, string TempStorage,bool Log)
        {
            this.Balance = balance;
            this.Bets = bets;
            this.Site = Site;
            this.BetSettings = OtherSettings;
            ///copy strategy
            this.DiceStrategy = DiceStrategy;
            this.DiceStrategy.NeedBalance += DiceStrategy_NeedBalance;
            this.DiceStrategy.OnNeedStats += DiceStrategy_OnNeedStats;
            this.DiceStrategy.Stop += DiceStrategy_Stop;
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
            TmpFileName = TempStorage + Site.R.Next()+".csv."+ Process.GetCurrentProcess().Id;
        }

        public void Save(string NewFile)
        {
           
            File.Move(TmpFileName, NewFile);
           
        }

        public void Start()
        {
            this.Stats = new SessionStats(true);
            this.SiteStats = CopyHelper.CreateCopy<SiteStats>(Site.Stats);
            SiteStats.Balance = Balance;
            Running = true;
            Stop = false;
            if (DiceStrategy is Strategies.ProgrammerMode)
            {
                (DiceStrategy as Strategies.ProgrammerMode).LoadScript();
            }
            new Thread(new ThreadStart(SimulationThread)).Start();
        }

        private void SimulationThread()
        {
            try
            {
                DiceBet NewBet = SimulatedBet(DiceStrategy.RunReset());
                this.Balance += (decimal)NewBet.Profit;
                Profit += (decimal)NewBet.Profit;
                while (TotalBetsPlaced < Bets && !Stop && Running)
                {
                    if (log)
                    {
                        bets.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}"
                        , TotalBetsPlaced, NewBet.Roll, NewBet.Chance, (NewBet.High ? ">" : "<"), NewBet.GetWin(Site) ? "win" : "lose", NewBet.TotalAmount, NewBet.Profit, this.Balance, Profit));
                    }
                    if (TotalBetsPlaced % 100000 == 0)
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
                    bool win = NewBet.GetWin(Site);
                    string Response = "";
                    if (BetSettings.CheckResetPreStats(NewBet, NewBet.GetWin(Site), Stats)) 
                    {
                        Reset = true;
                        NewBetObject = DiceStrategy.RunReset();
                    }
                    if (BetSettings.CheckStopPreStats(NewBet, NewBet.GetWin(Site), Stats, out Response))
                    {
                        this.Stop = (true);
                    }
                    Stats.UpdateStats(NewBet, win);
                    if (DiceStrategy is ProgrammerMode)
                    {
                        (DiceStrategy as ProgrammerMode).UpdateSessionStats(CopyHelper.CreateCopy<SessionStats>(Stats));
                        (DiceStrategy as ProgrammerMode).UpdateSiteStats(CopyHelper.CreateCopy<SiteStats>(SiteStats));
                        (DiceStrategy as ProgrammerMode).UpdateSite(CopyHelper.CreateCopy<SiteDetails>(Site.SiteDetails));
                    }
                    if (BetSettings.CheckResetPostStats(NewBet, NewBet.GetWin(Site), Stats))
                    {
                        Reset = true;
                        NewBetObject = DiceStrategy.RunReset();
                    }
                    if (BetSettings.CheckStopPOstStats(NewBet, NewBet.GetWin(Site), Stats, out Response))
                    {
                        Stop = true;
                    }
                    decimal withdrawamount = 0;
                    if (BetSettings.CheckWithdraw(NewBet, NewBet.GetWin(Site), Stats, out withdrawamount))
                    {
                        this.Balance -= withdrawamount;
                    }
                    if (BetSettings.CheckBank(NewBet, NewBet.GetWin(Site), Stats, out withdrawamount))
                    {
                        this.Balance -= withdrawamount;
                    }
                    if (BetSettings.CheckTips(NewBet, NewBet.GetWin(Site), Stats, out withdrawamount))
                    {
                        this.Balance -= withdrawamount;
                    }
                    bool NewHigh = false;
                    if (BetSettings.CheckResetSeed(NewBet, NewBet.GetWin(Site), Stats))
                    {
                        GenerateSeeds();
                    }
                    if (BetSettings.CheckHighLow(NewBet, NewBet.GetWin(Site), Stats, out NewHigh))
                    {
                        DiceStrategy.High = NewHigh;
                    }
                    if (!Reset)
                        NewBetObject = DiceStrategy.CalculateNextDiceBet(NewBet, win);
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
                Logger.DumpLog(e);
            }
        }

        

        void GenerateSeeds()
        {
            clientseed = Site.GenerateNewClientSeed();
            //new server seed
            //new client seed
            string serverseed = "";
            string Alphabet = "1234567890QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm";
            while (serverseed.Length<64)
            {
                serverseed += Alphabet[Site.R.Next(0, Alphabet.Length)];
            }
            this.serverseed = serverseed;
            //new server seed hash
            serverseedhash = Site.GetHash(serverseed);
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
            
            Lucky=Site.GetLucky(serverseedhash, serverseed, clientseed, (int)BetsWithSeed);
            
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
            betresult.Profit = betresult.GetWin(Site) ?  ((((100.0m - Site.Edge) / NewBet.Chance) * NewBet.Amount)-NewBet.Amount): -NewBet.Amount;
                
            return betresult;
        }

        private void DiceStrategy_Stop(object sender, Strategies.StopEventArgs e)
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
    }
}
