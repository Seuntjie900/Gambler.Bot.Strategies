using Gambler.Bot.Strategies.Strategies.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Limbo;
using System;

namespace Gambler.Bot.Strategies.Strategies
{
    public class Labouchere: BaseStrategy
    {
        public override string StrategyName { get; protected set; } = "Labouchere";
        //public string LabList { get; set; }
        public bool starthigh { get; set; } = true;
        public bool startlow { get { return !starthigh; } set { starthigh = !value; } }
        public List<decimal> BetList { get; set; } = new List<decimal>();
        public decimal[] SerializableBetList { get { return BetList.ToArray(); } set { BetList = new List<decimal>(value); } }
        List<decimal> LabList = new List<decimal>();

        public Labouchere(ILogger logger) : base(logger)
        {

        }

        public Labouchere()
        {
            
        }

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
        {
            decimal Lastbet = PreviousBet.TotalAmount;
            if (Win)
            {
                if (chkReverseLab)
                {
                    if (LabList.Count == 1)
                        LabList.Add(LabList[0]);
                    else
                        LabList.Add(LabList[0] + LabList[LabList.Count - 1]);
                }
                else if (LabList.Count > 1)
                {
                    LabList.RemoveAt(0);
                    LabList.RemoveAt(LabList.Count - 1);
                    if (LabList.Count == 0)
                    {
                        if (rdbLabStop)
                        {
                            CallStop("End of labouchere list reached");

                        }
                        else
                        {
                            RunReset(PreviousBet.Game);
                        }
                    }

                }
                else
                {
                    if (rdbLabStop)
                    {
                        CallStop("End of labouchere list reached");

                    }
                    else
                    {
                        LabList = BetList.ToArray().ToList<decimal>();
                        if (LabList.Count == 1)
                            Lastbet = LabList[0];
                        else if (LabList.Count > 1)
                            Lastbet = LabList[0] + LabList[LabList.Count - 1];
                    }
                }
            }
            else
            {
                //do laboucghere logic
                
                if (!chkReverseLab)
                {
                    if (LabList.Count == 1)
                        LabList.Add(LabList[0]);
                    else
                        LabList.Add(LabList[0] + LabList[LabList.Count - 1]);
                }
                else
                {
                    if (LabList.Count > 1)
                    {
                        LabList.RemoveAt(0);
                        LabList.RemoveAt(LabList.Count - 1);
                        if (LabList.Count == 0)
                        {
                            CallStop("Stopping: End of labouchere list reached.");

                        }
                    }
                    else
                    {
                        if (rdbLabStop)
                        {
                            CallStop("Stopping: End of labouchere list reached.");

                        }
                        else
                        {
                            LabList = BetList.ToArray().ToList<decimal>();
                            if (LabList.Count == 1)
                                Lastbet = LabList[0];
                            else if (LabList.Count > 1)
                                Lastbet = LabList[0] + LabList[LabList.Count - 1];
                        }
                    }
                }
                //end labouchere logic
            }

            if (LabList.Count == 1)
                Lastbet = LabList[0];
            else if (LabList.Count > 1)
                Lastbet = LabList[0] + LabList[LabList.Count - 1];
            else
            {
                if (rdbLabStop)
                {
                    CallStop("Stopping: End of labouchere list reached.");

                }
                else
                {
                    LabList = BetList.ToArray().ToList<decimal>();
                    if (LabList.Count == 1)
                        Lastbet = LabList[0];
                    else if (LabList.Count > 1)
                        Lastbet = LabList[0] + LabList[LabList.Count - 1];
                }
            }
            if (PreviousBet is DiceBet diceb && PreviousBet.Game == Games.Dice)
                return new PlaceDiceBet(Lastbet, High, diceb.Chance);
            if (PreviousBet is LimboBet limbob && PreviousBet.Game == Games.Limbo)
                return new PlaceLimboBet(Lastbet, limbob.Chance);
            else 
                throw new NotImplementedException("Strategy does not support this game.");
        }

        public override PlaceBet RunReset(Games Game)
        {
            decimal Amount = 0;
            LabList = BetList.ToArray().ToList<decimal>();
            if (LabList.Count == 1)
                Amount= LabList[0];
            else if (LabList.Count > 1)
                Amount= LabList[0] + LabList[LabList.Count - 1];
            High = starthigh;
            if (Game == Games.Dice)
                return new PlaceDiceBet(Amount, High, Chance);
            if (Game == Games.Limbo)
                return new PlaceLimboBet(Amount, 100/Chance);
            else
                throw new NotImplementedException("Strategy does not support this game.");
        }

        public bool rdbLabEnable { get; set; }

        public bool chkReverseLab { get; set; }

        public bool rdbLabStop { get; set; }
        public bool rdbLabReset { get=>!rdbLabStop; set=>rdbLabStop=!value; }
        public bool High { get ; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set ; }
        public decimal StartChance { get ; set ; }
    }
}
