decimal baseb = 0.00000001;
void CaluclateBet(dynamic PreviousBet, dynamic Win, dynamic NextBet)
{
    if (Win)
    {
        NextBet.Amount = baseb;
        NextBet.High = !NextBet.High;
    }
    else
    {
        NextBet.Amount = PreviousBet.TotalAmount * 2;
    }
    

}

void Reset(dynamic NextBet)
{
    NextBet.Amount = baseb;
    NextBet.Chance = 49.5;
    NextBet.High = True;
}