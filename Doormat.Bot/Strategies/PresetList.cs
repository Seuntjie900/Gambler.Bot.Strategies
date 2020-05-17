using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatBot.Strategies
{
    public class PresetList: BaseStrategy, iDiceStrategy
    {
        public class PresetBet
        {
            public decimal Amount { get; set; }
        }
        public class PresetDiceBet:PresetBet 
        {
            
            public bool? High { get; set; }
            public bool Switch { get; set; }
            public decimal? Chance { get; set; }
        }

        public override string StrategyName { get; protected set; } = "PresetList";
        int presetLevel = 0;
        public BindingList<PresetDiceBet> PresetBets { get; set; } = new BindingList<PresetDiceBet>();
        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            decimal Lastbet = PreviousBet.TotalAmount;
            if (Win)
            {
                switch (WinAction)
                {
                    case "Step": presetLevel += WinStep; break;
                    case "Stop": CallStop("Stop on win set in preset list."); break;
                    case "Reset": presetLevel = 0;break;

                }                
            }
            else
            {
                switch (LossAction)
                {
                    case "Step": presetLevel += LossStep; break;
                    case "Stop": CallStop("Stop on Loss set in preset list."); break;
                    case "Reset": presetLevel = 0; break;

                }                
            }
            if (presetLevel < 0)
                presetLevel = 0;
            if (presetLevel > PresetBets.Count - 1)
            {
                switch (EndAction)
                {
                    case "Step": while (presetLevel > PresetBets.Count - 1) 
                            presetLevel -= EndStep; break;
                    case "Stop": CallStop("End of preset list reached"); break;
                    case "Reset": presetLevel = 0; break;

                }                
            }

            if (presetLevel < PresetBets.Count)
            {
                Lastbet =SetPresetValues(presetLevel);
            }
            else
            {
                CallStop("It Seems a problem has occurred with the preset list values");
            }
            return new PlaceDiceBet(Lastbet, High, Chance);
        }

        public override PlaceDiceBet RunReset()
        {
            presetLevel = 0;
            decimal Lastbet = SetPresetValues(presetLevel);
            return new PlaceDiceBet(Lastbet, High, Chance);
        }

        decimal SetPresetValues(int Level)
        {
            if (PresetBets[Level] is PresetDiceBet dicebet)
            {
                Chance = dicebet.Chance ?? Chance;
                High = dicebet.High ?? High;
                High = dicebet.Switch ? !High : High;
            }
            return PresetBets[Level].Amount;


            /*decimal Lastbet = 0;
            decimal Betval = -1;
            string[] Vars = null;
            if (lstPresetList[Level].Contains("-"))
            {
                Vars = lstPresetList[Level].Split('-');
            }
            else if (lstPresetList[Level].Contains("/"))
            {
                Vars = lstPresetList[Level].Split('/');
            }
            else if (lstPresetList[Level].Contains("\\"))
            {
                Vars = lstPresetList[Level].Split('\\');
            }
            else
            {
                Vars = lstPresetList[Level].Split('&');
            }

            if (decimal.TryParse(Vars[0], out Betval))
            {
                Lastbet = Betval;
            }
            if (Vars.Length >= 2)
            {
                decimal chance = -1;
                if (decimal.TryParse(Vars[1], out chance))
                {
                    Chance=(decimal)(chance);
                }
                else
                {
                    if (Vars[1].ToLower() == "low" || Vars[1].ToLower() == "lo")
                        High=(false);
                    else if (Vars[1].ToLower() == "high" || Vars[1].ToLower() == "hi")
                    {
                        High=(true);
                    }
                }
                if (Vars.Length >= 3)
                {
                    if (decimal.TryParse(Vars[2], out chance))
                    {
                        Chance=(decimal)(chance);
                    }
                    else
                    {
                        if (Vars[2].ToLower() == "low" || Vars[2].ToLower() == "lo")
                            High=(false);
                        else if (Vars[2].ToLower() == "high" || Vars[2].ToLower() == "hi")
                        {
                            High=(true);
                        }
                    }
                }
            }
            else
            {
                CallStop("Invalid bet inpreset list");
            }
            return Lastbet;*/
        }


        public string WinAction { get; set; } = "Step";

        public int WinStep { get; set; } = -1;

        public string LossAction { get; set; } = "Step";
        public string EndAction { get; set; } = "Stop";

        public int LossStep { get; set; } = 1;

        public int EndStep { get; set; } = -1;

        public bool High { get ; set ; }
        public decimal Amount { get ; set ; }
        public decimal Chance { get ; set ; }
        public decimal StartChance { get ; set ; }
    }


}
