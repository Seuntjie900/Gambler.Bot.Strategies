var base = 0.00000001;
function DoDiceBet(PreviousBet, Win, NextBet) {
    if (Win) {
        NextBet.Amount = base;
        NextBet.High = !NextBet.High;
    }
    else {
        NextBet.Amount = PreviousBet.TotalAmount * 2;
    }
    if (Stats.Profit > SiteDetails.Wagered * 0.0001) {
        Withdraw('your address here', Stats.Balance * 0.01);
    }
    
}

function ResetDice(NextBet) {
    NextBet.Amount = base;
    NextBet.Chance = 49.5;
    NextBet.High = True;
}