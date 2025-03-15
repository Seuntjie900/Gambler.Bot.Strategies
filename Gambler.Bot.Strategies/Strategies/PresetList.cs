using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.Strategies.Strategies.PresetListModels;
using Gambler.Bot.Common.Games;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Gambler.Bot.Strategies.Strategies
{
    public class PresetList: BaseStrategy, iDiceStrategy
    {
        

        public override string StrategyName { get; protected set; } = "PresetList";
        int presetLevel = 0;

        public PresetList(ILogger logger) : base(logger)
        {

        }
        public PresetList()
        {
            
        }
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
            if (presetLevel >= PresetBets.Count - 1)
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

namespace Gambler.Bot.Strategies.Strategies.PresetListModels
{
    public class PresetBet
    {
        public decimal Amount { get; set; }
    }
    public class PresetDiceBet : PresetBet
    {

        public bool? High { get; set; }
        public bool Switch { get; set; }
        public decimal? Chance { get; set; }
    }
}