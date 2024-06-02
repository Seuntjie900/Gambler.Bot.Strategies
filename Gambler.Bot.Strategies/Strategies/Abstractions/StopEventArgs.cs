using System;

namespace Gambler.Bot.AutoBet.Strategies.Abstractions
{
    public class StopEventArgs : EventArgs
    {
        public string Reason { get; set; }

        public StopEventArgs(string Reason)
        {
            this.Reason = Reason;
        }
    }
}
