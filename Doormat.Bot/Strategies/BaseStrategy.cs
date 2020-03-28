using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;

namespace DoormatBot.Strategies
{
    public abstract class BaseStrategy
    {
        /// <summary>
        /// The strategies name
        /// </summary>
        public string StrategyName { get; protected set; }

        public PlaceBet CalculateNextBet(Bet PreviousBet, bool Win)
        {
            if (PreviousBet is DiceBet)
                return CalculateNextDiceBet(PreviousBet as DiceBet, Win);
            else if (PreviousBet is CrashBet)
                return CalculateNextCrashBet(PreviousBet as CrashBet, Win);
            else if (PreviousBet is RouletteBet)
                return CalculateNextRouletteBet(PreviousBet as RouletteBet, Win);
            else if (PreviousBet is PlinkoBet)
                return CalculateNextPlinkoBet(PreviousBet as PlinkoBet, Win);
            return null;
        }

        /// <summary>
        /// The main logic for the strategy. This is called in between every bet.
        /// </summary>
        /// <param name="PreviousBet">The bet details for the last bet that was placed</param>
        /// <returns>Bet details for the bet to be placed next.</returns>
        public virtual PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win) { throw new NotImplementedException(); }

        public virtual PlaceCrashBet CalculateNextCrashBet(CrashBet PreviousBet, bool Win) { throw new NotImplementedException(); }

        public virtual PlaceRouletteBet CalculateNextRouletteBet(RouletteBet PreviousBet, bool Win) { throw new NotImplementedException(); }

        public virtual PlacePlinkoBet CalculateNextPlinkoBet(PlinkoBet PreviousBet, bool Win) { throw new NotImplementedException(); }


        /// <summary>
        /// Reset the betting strategy
        /// </summary>
        /// <returns></returns>
        public abstract PlaceDiceBet RunReset();

        /// <summary>
        /// Gets the users balance from the site
        /// </summary>
        protected decimal Balance {get{return GetBalance();}}

        public bool High { get; set; } = true;
        public decimal Amount { get; set; } = 0.00000100m;
        public decimal Chance { get; set; } = 49.5m;

        public virtual void LoadString(string Folder)
        {

        }

        protected decimal GetBalance()
        {
            if (NeedBalance != null)
                return NeedBalance();
            else
                return 0;
        }

        public delegate decimal dNeedBalance();
        public event dNeedBalance NeedBalance;

        public delegate SessionStats dNeedStats(object sender, EventArgs e);
        public event dNeedStats OnNeedStats;

        

        public SessionStats Stats
        {
            get { return OnNeedStats?.Invoke(this, new EventArgs()); }
            
        }


        protected void CallStop(string Reason)
        {
            if (Stop != null)
                Stop(this, new StopEventArgs(Reason));
        }


        public delegate void dStop(object sender, StopEventArgs e);
        public event dStop Stop;

        public virtual void OnError(ErrorEventArgs ErrorDetails)
        {
            ErrorDetails.Handled = false;
        }
    }
    public class StopEventArgs:EventArgs
    {
        public string Reason { get; set; }

        public StopEventArgs(string Reason)
        {
            this.Reason = Reason;
        }
    }
    
}
