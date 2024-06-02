var base = 0.00000001;
function DoDiceBet(PreviousBet, Win, NextBet) {
    if (Win) {
        NextBet.Amount = base;
        NextBet.High = !NextBet.High;
    }
    else {
        NextBet.Amount = PreviousBet.TotalAmount * 2;
    }    
    
}

function ResetDice(NextBet) {
    NextBet.Amount = base;
    NextBet.Chance = 49.5;
    NextBet.High = true;
}