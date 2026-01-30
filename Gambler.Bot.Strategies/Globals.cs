using Gambler.Bot.Strategies.Helpers;
using Gambler.Bot.Common.Helpers;
using System;
using Gambler.Bot.Common.Games;

namespace Gambler.Bot.Core
{
    public class Globals
    {
        public SiteStats SiteStats { get; set; }
        public SiteDetails SiteDetails { get; set; }
        public SessionStats Stats { get; set; }
        public object NextBet { get; set; }
        public object ErrorArgs { get; set; }
        public object PreviousBet { get; set; }
        public bool Win { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public Action<string, decimal> Withdraw { get; set; }
        public Action<decimal> Bank { get; set; }
        public Action<decimal> Invest{ get; set; }
        public Action<string, decimal> Tip{ get; set; }
        public Action ResetSeed{ get; set; }
        public Action<string> Print{ get; set; }
        public Action<decimal, long, bool> RunSim{ get; set; }
        public Action ResetStats{ get; set; }
        public Func<string, int, object> Read{ get; set; }
        public Func<string, int, string, string, string, object>  Readadv { get; set; }
        public Action Alarm{ get; set; }
        public Action Ching{ get; set; }
        public Action ResetBuiltIn{ get; set; }
        public Action<string> ExportSim { get; set; }
        public Action Stop { get; set; }
        public Action<string> SetCurrency { get; set; }
        public Func<string, PlaceBet> ChangeGame { get; set; }
        public bool InSimulation { get; set; }
        public Action<int> Sleep { get; set; }
        public bool MaintainBetDelay { get; set; }
        public int BetDelay { get; set; }
        public Action<bool,decimal> SetBotSpeed { get; set; }
    }
}
