base = 0.00000001
function DoDiceBet(PreviousBet, Win, NextBet)
if Win then
NextBet.Amount=base
NextBet.High=NextBet.High
else
NextBet.Amount=PreviousBet.TotalAmount*2
end
if Stats.Profit>SiteStats.Wagered*0.0001 then
Withdraw('your address here',Stats.Balance*0.01)
end
end

function ResetDice(NextBet)
NextBet.Amount=base
NextBet.Chance=49.5
NextBet.High=true
end