using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoormatBot.Helpers;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using Microsoft.Extensions.Logging;

namespace DoormatBot.Strategies
{
    public abstract class BaseStrategy
    {
        protected readonly ILogger _Logger;

        protected BaseStrategy(ILogger logger)
        {
            _Logger = logger;
        }

        private BaseStrategy WorkingSet = null;
        /// <summary>
        /// The strategies name
        /// </summary>
        public abstract string StrategyName { get; protected set; }

        public PlaceBet CalculateNextBet(Bet PreviousBet, bool Win)
        {
            if (WorkingSet != null)
                return WorkingSet.CalculateNextBet(PreviousBet, Win);
            else
            {
                if (PreviousBet is DiceBet && this is iDiceStrategy dc)
                    return dc.CalculateNextDiceBet(PreviousBet as DiceBet, Win);
                else if (PreviousBet is CrashBet)
                    return CalculateNextCrashBet(PreviousBet as CrashBet, Win);
                else if (PreviousBet is RouletteBet)
                    return CalculateNextRouletteBet(PreviousBet as RouletteBet, Win);
                else if (PreviousBet is PlinkoBet)
                    return CalculateNextPlinkoBet(PreviousBet as PlinkoBet, Win);
            }
            return null;
        }


        public virtual PlaceCrashBet CalculateNextCrashBet(CrashBet PreviousBet, bool Win) { throw new NotImplementedException(); }

        public virtual PlaceRouletteBet CalculateNextRouletteBet(RouletteBet PreviousBet, bool Win) { throw new NotImplementedException(); }

        public virtual PlacePlinkoBet CalculateNextPlinkoBet(PlinkoBet PreviousBet, bool Win) { throw new NotImplementedException(); }


        /// <summary>
        /// Indicates to the strategy that automated betting is starting.
        /// </summary>
        /// <returns></returns>
        public PlaceBet Start()
        {
            if (!(this is ProgrammerMode))
            {
                /*WorkingSet = CopyHelper.CreateCopy(this.GetType(), this) as BaseStrategy;
                WorkingSet.NeedBalance += WorkingSet_NeedBalance;
                WorkingSet.OnNeedStats += WorkingSet_OnNeedStats;
                WorkingSet.Stop += WorkingSet_Stop;*/
                return RunReset();
            }
            else
            {
                return RunReset();
            }
        }

        private void WorkingSet_Stop(object sender, StopEventArgs e)
        {
            Stop?.Invoke(sender, e);
        }

        private SessionStats WorkingSet_OnNeedStats(object sender, EventArgs e)
        {
            return OnNeedStats?.Invoke(sender, e);
        }

        private decimal WorkingSet_NeedBalance()
        {
           return NeedBalance?.Invoke()??0;
        }

        /// <summary>
        /// Reset the betting strategy
        /// </summary>
        /// <returns></returns>
        public abstract PlaceDiceBet RunReset();

        /// <summary>
        /// Gets the users balance from the site
        /// </summary>
        protected decimal Balance {get{return GetBalance();}}
               

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
    
    public interface iDiceStrategy
    {
        public bool High { get; set; }
        public decimal Amount { get; set; }
        public decimal Chance { get; set; } 
        public decimal StartChance { get; set; }

        /// <summary>
        /// The main logic for the strategy. This is called in between every bet.
        /// </summary>
        /// <param name="PreviousBet">The bet details for the last bet that was placed</param>
        /// <returns>Bet details for the bet to be placed next.</returns>
        public PlaceDiceBet CalculateNextDiceBet(DiceBet PreviousBet, bool Win);
    }
}
