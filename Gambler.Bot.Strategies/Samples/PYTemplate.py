base = 0.00000001
def CalculateBet():
    if (Win):
        NextBet.Amount = base;
        NextBet.High = NextBet.High==True;    
    else:
        NextBet.Amount = PreviousBet.TotalAmount * 2;

def Reset():
    NextBet.Amount = base;
    NextBet.Chance = 49.5;
    NextBet.High = True;
