using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatBot.Strategies
{
    public class PresetList: BaseStrategy
    {

        int presetLevel = 0;
        List<string> lstPresetList = new List<string>();
        public override PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win)
        {
            decimal Lastbet = PreviousBet.TotalAmount;
            if (Win)
            {
                if (rdbPresetWinStep)
                {
                    presetLevel += nudPresetWinStep;
                }
                else if (rdbPresetWinReset)
                {
                    presetLevel = 0;
                }
                else
                {
                    presetLevel = 0;
                    CallStop("Preset List bet won.");

                }
            }
            else
            {
                if (rdbPresetLossStep)
                {
                    presetLevel += nudPresetLossStep;
                }
                else if (rdbPresetLossReset)
                {
                    presetLevel = 0;
                }
                else
                {
                    presetLevel = 0;
                    CallStop("Preset List bet lost.");

                }
            }
            if (presetLevel < 0)
                presetLevel = 0;
            if (presetLevel > lstPresetList.Count - 1)
            {
                if (rdbPresetEndStop)
                {
                    CallStop("End of preset list reached.");

                }
                else if (rdbPresetEndStep)
                {
                    while (presetLevel > lstPresetList.Count - 1)
                    {
                        presetLevel -= nudPresetEndStep;
                    }
                }
                else
                {
                    presetLevel = 0;
                }
            }

            if (presetLevel < lstPresetList.Count)
            {
                Lastbet =SetPresetValues(presetLevel);
            }
            else
            {
                CallStop("It Seems a problem has occurred with the preset list values");
            }
            return new PlaceDiceBet(Lastbet, High, PreviousBet.Chance);
        }

        public override PlaceDiceBet RunReset()
        {
            throw new NotImplementedException();
        }

        decimal SetPresetValues(int Level)
        {
            decimal Lastbet = 0;
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
            return Lastbet;
        }


        public bool rdbPresetWinStep { get; set; }

        public int nudPresetWinStep { get; set; }

        public bool rdbPresetWinReset { get; set; }

        public bool rdbPresetLossStep { get; set; }

        public int nudPresetLossStep { get; set; }

        public bool rdbPresetLossReset { get; set; }

        public bool rdbPresetEndStop { get; set; }

        public bool rdbPresetEndStep { get; set; }

        public int nudPresetEndStep { get; set; }
    }


}
