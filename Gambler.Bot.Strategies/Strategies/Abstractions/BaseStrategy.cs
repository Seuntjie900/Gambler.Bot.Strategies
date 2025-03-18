using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Common.Games;
using Microsoft.Extensions.Logging;
using System;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Plinko;
using Gambler.Bot.Common.Games.Roulette;
using Gambler.Bot.Common.Games.Limbo;

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
                return NextBet(PreviousBet, Win);
            }
            return null;
        }

        protected PlaceBet CreateEmptyPlaceBet(Games game)
        {
            switch (game)
            {
                case Games.Dice:
                    return new PlaceDiceBet(0, false, 0);
                    break;
                case Games.Limbo:
                    return new PlaceLimboBet(0, 0);
                    break;
            };
            return null;
        }

        protected abstract PlaceBet NextBet(Bet PreviousBet, bool Win);

        /// <summary>
        /// Indicates to the strategy that automated betting is starting.
        /// </summary>
        /// <returns></returns>
        public PlaceBet Start(Games game)
        {
            if (!(this is IProgrammerMode))
            {
                /*WorkingSet = CopyHelper.CreateCopy(this.GetType(), this) as BaseStrategy;
                WorkingSet.NeedBalance += WorkingSet_NeedBalance;
                WorkingSet.OnNeedStats += WorkingSet_OnNeedStats;
                WorkingSet.Stop += WorkingSet_Stop;*/
                return RunReset(game);
            }
            else
            {
                return RunReset(game);
            }
        }

        /// <summary>
        /// Reset the betting strategy
        /// </summary>
        /// <returns></returns>
        public abstract PlaceBet RunReset(Games game);

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
