(1) How to run:
	1.1: Windows: Double Click on the DoormatControllers.exe file inside of the DiceBot folder.
	1.2: Linux: Execute the DoormatControllers file from the terminal.
	1.3: OSX: I don't know? Try double clicking on the DoormatControllers file inside the DiceBot folder or something. Or run it from the terminal? Can you do that in OSX?

(2) There are example configuration files included with the packages. While default configuration files are created when running
the bot for the first time, the console ui does not provide a way to configure all of the settings. The sample files
are there to provide examples of how certain settings would look like so you could compile it yourself.


(3) Triggers
	(3.1) Currently, there are no way to define triggers from the console UI and they can only be specified in the settings files.
	Triggers are processed after every bet and when the condition set in the trigger is met, it is executed. Triggers are used
	to determine whether the bot should stop, reset, notify, withdraw, tip etc. Every trigger has a specified action to execute
	the conditions set in the trigger are met. These actions are:
		0: Alarm - Notifications Only
		1: Chime - Notifications Only  
		2: Email - Notifications Only
		3: Popup - Notifications Only
		4: Stop - Bet Settings Only
		5: Reset - Bet Settings Only
		6: Withdraw - Bet Settings Only
		7: Tip - Bet Settings Only
		8: Invest - Bet Settings Only
		9: Bank - Bet Settings Only
		10: ResetSeed - Bet Settings Only
	
	Either the action name or the ascotiated number can be specified in the json file.
	
	(3.2) Other lists to keep in mind when working with triggers:
		
		CompareAgainst:
			0:Value
			1:Percentage
			2:Property
			
		TriggerComparison:
			0: Equals
			1: LargerThan
			2: SmallerThan
			3: LargerOrEqualTo
			4: SmallerOrEqualTo
			5: Modulus	
	
	(3.3) How does the triggers work?
		Triggers have the following properties that need to be set:
		
			TriggerAction Action - The action to take when the conditions are met
			bool Enabled - Whether or not the test the trigger. Triggers disabled is not tested and cannot execute.
			string TriggerProperty - The name of the property of the stats class to use for the trigger.
			CompareAgainst TargetType - Where the values that is compared against comes from
			string Target - When compare against is set to Value, the trigger property is compared directly to the numeric equivalent of Target. When set to Property or Percentage, 
				Target must specify the name of a property from the stats object. When Property, the TriggerProperty is compared to the property value specified by target. 
				When set to Percentage, the TriggerProperty is calculated as a percentage of the property value specified by target.
			TriggerComparison Comparison - What kind of comparison is done between the TriggerProperty and Target. Specify an item from TriggerComparison
			decimal Percentage - Only used when CompareAgainst is set to Percentage. (((TriggerProperty/Target)*100) (Comparison) Percentage). 
			CompareAgainst ValueType - When the action requires a value to be returned, like in the case of tipping or withdrawal, 
				this specifies where the value is acquired from. 
			string ValueProperty -  The value that is used for the action if value type is set to property or percentage.
			decimal ValueValue - If the ValueProperty is set to value, this value is used for the action. If set the percentage,
				this percentage of the ValueProperty value is used for the action.
			string Destination - Specify any extra information needed by the action, for example the username to tip to or address to withdraw to.
			
	(3.4) Examples:
		
			This example will trigger the alarm sound when the session profit is larger than or equal to 1.5.
			{  
			   "Action":"Alarm",
			   "Enabled":True,
			   "TriggerProperty":"Profit",
			   "TargetType":"Value",
			   "Target":"1.5",
			   "Comparison":"LargerOrEqualTo"
			}

			This trigger will stop the bot if the ratio of wins vs losses drops below 10%. So when (wins/losses)*100<10.0
			{  
			   "Action":"Stop",
			   "Enabled":True,
			   "TriggerProperty":"Wins",
			   "TargetType":"Percentage",
			   "Target":"Losses",
			   "Comparison":"SmallerThan",
			   "Percentage":10
			}
			
			
			This will tip 0.1% of the session profit to account Seuntjie every 1000 bets.
			{  
			   "Action":"Tip",
			   "Enabled":True,
			   "TriggerProperty":"Bets",
			   "TargetType":"Value",
			   "Target":"1000",
			   "Comparison":"Modulus",
			   "ValueType":"Percentage",
			   "ValueProperty":"Profit",
			   "ValueValue":"0.1",
			   "Destination":"Seuntjie"
			}
			
(4) Personal Settings
	(4.1) The sample file SAMPLE_PersonalSettings.json is an example of the personal settings file. Rename the file to personalsettings.json 
	This file illustrates how to configure nondefault error handling, as well as notifications. Note that in the current version, notifications
	are not yet implemented other than writing a line to the console of the notification.
	
	(4.2) Error Handling
		DiceBot v4 allows you to specify behaviour for certain errors. There's still some work to be done here (specifically to catagorize the errors
		in the site interface), but the basic functionality is available. To specify error behaviour, you need to add an object into the
		ErrorSettings array in the personal settings file. The object should have a Type and an Action property.
			
			Avaialble actions:
				2: Resume - only available for non bet related errors.
				3: Stop 
				4: Reset 
				5: Retry 
				
			Available Types:
				0: InvalidBet,
				1: BalanceTooLow,
				2: ResetSeed,
				3: Withdrawal,
				4: Tip,
				5: NotImplemented,
				6: Other,
				7: BetMismatch,
				8: Unknown		
		
		When specifying an error settings object, either the ascoiated number or the text can be used, for example:
			
			{"Type":"BalanceTooLow","Action":"Reset"}
			means the same thing as 
			{"Type":1,"Action":4}
		
		This tells the bot that whenever your balance is too low to make the next bet, it needs to reset and restart your betting pattern.
		
		You can specify one (1) action for every error type. If you specify more than one, the latter in the file will be used. 
		
	(4.3) Notifications
		Notifications are driven by Triggers. See the triggers section above. Only triggers with Notification actions can be used here. Other triggers will be
		tested after every bet, but the action will not be executed.
		
(5) Bet Settings
	(5.1) An example configuration file for bet settings is provided under the name SAMPLE_betsettings.json. Rename this file to betsettings.json to use it. 
		It contains a martingale setting with default settings and 2 triggers configured and enabled. You can generate new bet settings file from the console ui
		by going to strategy, then selecting new. It doesn't discard previously setup triggers, only strategy specific settings are discarded. 
	(5.2) Bet settings have a list of triggers that can be used to manipulate the bot, including tipping, withdrawing, resetting etc. See the triggers section above.
	Trigger actions marked as notifications cannot be used here.
	
(6) Programmer mode
	Sample scripts for the programmer modes are provided, 1 for each language available, all doing the exact same thing, namely martingale. These files are name SAMPLEJS.js, SAMPLELUA.lua and SAMPLECS.cs.
	In the strategy mode, after selecting the applicable strategy, specify the file name in the betsetting.json file (or through the console ui) to the script you want to use.
	The script file is executed any time the start command is used. There is no CLI to interact with the programmer modes just yet, but it's a planned feature.
	
(7) SessionStats Class Definition: 
public class SessionStats
{
	
	public long RunningTime { get; set; }
	public long Losses { get; set; }
	public long Wins { get; set; }
	public long Bets { get; set; }
	public long LossStreak { get; set; }
	public long WinStreak { get; set; }
	public decimal Profit { get; set; }
	public decimal Wagered { get; set; }
	public long WorstStreak { get; set; }
	public long WorstStreak3 { get; set; }
	public long WorstStreak2 { get; set; }
	public long BestStreak { get; set; }
	public long BestStreak3 { get; set; }
	public long BestStreak2 { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public long laststreaklose { get; set; }
	public long laststreakwin { get; set; }
	public decimal LargestBet { get; set; }
	public decimal LargestLoss { get; set; }
	public decimal LargestWin { get; set; }
	public decimal luck { get; set; }
	public decimal AvgWin { get; set; }        
	public decimal AvgLoss { get; set; }
	public decimal AvgStreak { get; set; }
	public decimal CurrentProfit { get; set; }
	public decimal StreakProfitSinceLastReset { get; set; }
	public decimal StreakLossSinceLastReset { get; set; }
	public decimal ProfitSinceLastReset { get; set; }
	public long winsAtLastReset { get; set; }
	public long NumLossStreaks { get; set; }
	public long NumWinStreaks { get; set; }
	public long NumStreaks { get; set; }
	public decimal PorfitSinceLimitAction { get; set; }
}

(8) SiteDetails Class Definition: 
public class SiteDetails
{
	public string name { get; set; }
	public decimal edge { get; set; }
	public decimal maxroll { get; set; }
	public bool cantip { get; set; }
	public bool tipusingname { get; set; }
	public bool canwithdraw { get; set; }
	public bool canresetseed { get; set; }
	public bool caninvest { get; set; }
	public string siteurl { get; set; }
	public long Wins { get; set; }
	public long Losses { get; set; }
	public decimal Profit { get; set; }
	public decimal Wagered { get; set; }
	public decimal Balance { get; set; }
	public long Bets { get; set; }
	
}

public class PlaceDiceBet
{
	/// <summary>
	/// Amount to be bet
	/// </summary>
	public decimal Amount { get; set; }
	/// <summary>
	/// Bet high when true, low when false
	/// </summary>
	public bool High { get; set; }
	/// <summary>
	/// The chance to place the bet at
	/// </summary>
	public decimal Chance { get; set; }
}

public class DiceBet
    {
       public decimal TotalAmount { get; set; }
        public decimal Date { get; set; }
		public DateTime DateValue { get { return SQLBase.DateFromDecimal(Date); } set { Date = SQLBase.DateToDecimal(value); } }
        public string BetID { get; set; }
        public decimal Profit { get; set; }
        public long Userid { get; set; }
        public string Currency { get; set; }
        public string Guid { get; set; }
        public decimal Roll { get; set; }
        public bool High { get; set; }
        public decimal Chance { get; set; }
        public long Nonce { get; set; }
        public string ServerHash { get; set; }
        public string ServerSeed { get; set; }
        public string ClientSeed { get; set; }
        public override PlaceBet CreateRetry()
        {
            return new PlaceDiceBet(TotalAmount, High, Chance);
        }
        public override bool GetWin(BaseSite Site)
        {
            return (((bool)High ? (decimal)Roll > (decimal)Site.MaxRoll - (decimal)(Chance) : (decimal)Roll < (decimal)(Chance)));
        }
    }