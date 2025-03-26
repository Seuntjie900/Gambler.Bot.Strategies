base = 0.00000001
function CalculateBet(PreviousBet, Win, NextBet)
	if Win then
		NextBet.Amount=base
		NextBet.High=NextBet.High
	else
		NextBet.Amount=PreviousBet.TotalAmount*2
	end
end

function Reset(NextBet)
	NextBet.Amount=base
	NextBet.Chance=49.5
	NextBet.High=true
end