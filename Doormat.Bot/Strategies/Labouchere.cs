using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatBot.Strategies
{
    public class Labouchere: BaseStrategy
    {
        List<decimal> OrigList = new List<decimal>();
        List<decimal> LabList = new List<decimal>();
        public override PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            decimal Lastbet = PreviousBet.TotalAmount;
            if (Win)
            {
                if (rdbLabEnable)
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
                                RunReset();
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
                            LabList = OrigList.ToArray().ToList<decimal>();
                            if (LabList.Count == 1)
                                Lastbet = LabList[0];
                            else if (LabList.Count > 1)
                                Lastbet = LabList[0] + LabList[LabList.Count - 1];
                        }
                    }
                }


            }
            else
            {
                //do laboucghere logic
                if (rdbLabEnable)
                {
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
                                LabList = OrigList.ToArray().ToList<decimal>();
                                if (LabList.Count == 1)
                                    Lastbet = LabList[0];
                                else if (LabList.Count > 1)
                                    Lastbet = LabList[0] + LabList[LabList.Count - 1];
                            }
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
                    LabList = OrigList.ToArray().ToList<decimal>();
                    if (LabList.Count == 1)
                        Lastbet = LabList[0];
                    else if (LabList.Count > 1)
                        Lastbet = LabList[0] + LabList[LabList.Count - 1];
                }
            }
            return new PlaceDiceBet(Lastbet, High, PreviousBet.Chance);
        }

        public override PlaceDiceBet RunReset()
        {
            decimal Amount = 0;
            LabList = OrigList.ToArray().ToList<decimal>();
            if (LabList.Count == 1)
                Amount= LabList[0];
            else if (LabList.Count > 1)
                Amount= LabList[0] + LabList[LabList.Count - 1];
            return new PlaceDiceBet(Amount, High, (decimal)Chance);
        }

        public bool rdbLabEnable { get; set; }

        public bool chkReverseLab { get; set; }

        public bool rdbLabStop { get; set; }
    }
}
