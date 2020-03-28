decimal baseb = 0.00000001;
void DoDiceBet(dynamic PreviousBet, dynamic Win, dynamic NextBet)
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
    if (Stats.Profit > SiteDetails.Wagered * 0.0001)
    {
        Withdraw("your address here", Stats.Balance * 0.01);
    }

}

void  ResetDice(dynamic NextBet)
{
    NextBet.Amount = baseb;
    NextBet.Chance = 49.5;
    NextBet.High = True;
}