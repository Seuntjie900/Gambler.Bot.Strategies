using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Common.Games;
using Microsoft.Extensions.Logging;
using System;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Plinko;
using Gambler.Bot.Common.Games.Roulette;

namespace Gambler.Bot.Strategies.Strategies.Abstractions
{
    public abstract class BaseStrategy
    {
        protected ILogger _Logger;

        protected BaseStrategy(ILogger logger)
        {
            _Logger = logger;
        }
        protected BaseStrategy()
        {

        }

        public void SetLogger(ILogger logger)
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
                if (PreviousBet is DiceBet db && this is iDiceStrategy dc)
                    return dc.CalculateNextDiceBet(db, Win);
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
            if (!(this is IProgrammerMode))
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

        /// <summary>
        /// Reset the betting strategy
        /// </summary>
        /// <returns></returns>
        public abstract PlaceDiceBet RunReset();

        /// <summary>
        /// Gets the users balance from the site
        /// </summary>
        protected decimal Balance { get { return GetBalance(); } }


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

        public virtual void OnError(BotErrorEventArgs ErrorDetails)
        {
            ErrorDetails.Handled = false;

        }
    }
}
