base = 0.00000001
def DoDiceBet(PreviousBet, Win, NextBet):
    if (Win):
        NextBet.Amount = base;
        NextBet.High = NextBet.High==True;    
    else:
        NextBet.Amount = PreviousBet.TotalAmount * 2;

def ResetDice(NextBet):
    NextBet.Amount = base;
    NextBet.Chance = 49.5;
    NextBet.High = True;
