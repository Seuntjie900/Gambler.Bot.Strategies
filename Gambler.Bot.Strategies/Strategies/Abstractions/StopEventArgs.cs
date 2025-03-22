using System;

namespace Gambler.Bot.Strategies.Strategies.Abstractions
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
