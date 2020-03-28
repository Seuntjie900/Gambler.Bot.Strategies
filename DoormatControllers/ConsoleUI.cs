using DoormatBot;
using DoormatBot.Helpers;
using DoormatBot.Strategies;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace DoormatControllers
{
    class ConsoleUI
    {
        Stopwatch SimTimer = new Stopwatch();
        Doormat DiceBot;

        public void Start()
        {

            DiceBot = new Doormat();
            DiceBot.OnSiteBetFinished += DiceBot_OnSiteDiceBetFinished;
            DiceBot.OnSiteAction += DiceBot_OnSiteAction;
            DiceBot.OnSiteChat += DiceBot_OnSiteChat;
            DiceBot.OnSiteError += DiceBot_OnSiteError;
            DiceBot.OnSiteLoginFinished += DiceBot_OnSiteLoginFinished;
            DiceBot.OnSiteNotify += DiceBot_OnSiteNotify;
            DiceBot.OnSiteRegisterFinished += DiceBot_OnSiteRegisterFinished;
            DiceBot.OnSiteStatsUpdated += DiceBot_OnSiteStatsUpdated;
            DiceBot.NeedConstringPassword += DiceBot_NeedConstringPassword;
            DiceBot.NeedKeepassPassword += DiceBot_NeedKeepassPassword;
            if (File.Exists("personalsettings.json"))
            {
                try
                {
                    DiceBot.LoadPersonalSettings("personalsettings.json");

                }
                catch (Exception e)
                {
                    Logger.DumpLog(e);
                }
            }
            if (DiceBot.PersonalSettings != null)
            {
                if (string.IsNullOrWhiteSpace(DiceBot.PersonalSettings.EncrConnectionString))
                {
                    ConfigureDatabase();
                }
            }

            if (File.Exists("betsettings.json"))
            {
                try
                {
                    DiceBot.LoadBetSettings("betsettings.json");
                }
                catch (Exception e)
                {
                    Logger.DumpLog(e);
                }
            }
            SelectSite();
            PrintCurrentActions();
            string Input = Console.ReadLine();
            while (Input.ToLower() != "exit")
            {
                try
                {
                    switch (Input.ToLower())
                    {
                        case "login": Login(); break;
                        case "site": SelectSite(); break;
                        case "currency": SelectCurrency(); break;
                        case "logout": Logout(); break;
                        case "sitestats": PrintSiteStats(); break;
                        case "strategy": Strategy(); break;
                        case "stats": Stats(); break;
                        case "start":
                            DiceBot.SavePersonalSettings("personalsettings.json");
                            DiceBot.SaveBetSettings("betsettings.json");
                            DiceBot.StartDice();
                            break;
                        case "stop": DiceBot.StopDice("Stop received from console."); break;
                        case "sim": Simulate(); break;
                        case "betsettings": BetSettings(); break;
                        case "personalsettings": PersonalSetting(); break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                PrintCurrentActions();
                Input = Console.ReadLine();
            }
            try
            {
                DiceBot.SavePersonalSettings("personalsettings.json");
                DiceBot.SaveBetSettings("betsettings.json");
                DiceBot.CurrentSite.Disconnect();
            }
            catch (Exception e)
            {
                Logger.DumpLog(e);
            }
        }

        private void DiceBot_NeedKeepassPassword(object sender, PersonalSettings.GetConstringPWEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pw))
            {
                Console.WriteLine("DiceBot needs to open your KeePass database.");
                Console.Write("Password: ");
                string Result = Console.ReadLine();
                pw = e.Password = Result;
                Console.Clear();
            }
            else
                e.Password = pw;
        }

        string pw = "";
        private void DiceBot_NeedConstringPassword(object sender, PersonalSettings.GetConstringPWEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pw))
            {
                Console.WriteLine("DiceBot needs to decrypt your connection string.");
                Console.Write("Password: ");
                string Result = Console.ReadLine();
                pw = e.Password = Result;
                Console.Clear();
            }
            else
                e.Password= pw;
        }

        void ConfigureDatabase()
        {
            while (true)
            {
                string provider = "";


                if (string.IsNullOrWhiteSpace(DiceBot.PersonalSettings.EncrConnectionString))
                    Console.WriteLine("It seems like this is the first time you're running DiceBot. Please configure a database to store your bets in.\n");
                Console.WriteLine("(1) SQLite (Default)\n(2) MS SQL Server\n(3) MYSQL\n(4) Mongo\n(5) Post Gre\n\nSelect database provider:");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                    default: provider = "SQLite"; break;
                    case "2": provider = "MSSQL"; break;
                    case "3": provider = "MYSQL"; break;
                    case "4": provider = "Mongo"; break;
                    case "5": provider = "PostGRE"; break;
                }
                Console.WriteLine("Selected Provider: " + provider);
                Console.WriteLine("Please provide the following DB settings:");
                Console.Write("Data Source (File name or database server): ");
                string datasource = Console.ReadLine();
                string Constring = "";
                if (input != "1")
                {
                    Console.Write("Initial Catalog (Database name):");
                    string dbname = Console.ReadLine();
                    Console.Write("DB Username: ");
                    string Username = Console.ReadLine();
                    Console.Write("DB Password: ");
                    string Password = Console.ReadLine();
                    Constring = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};persist security info=true;", datasource, dbname, Username, Password);
                    DiceBot.PersonalSettings.Provider = provider;

                }
                else
                {
                    Constring = string.Format("Data Source={0};Version=3;Compress=True;", datasource);
                    DiceBot.PersonalSettings.Provider = provider;

                }
                Console.WriteLine("Enter a password to encrypt your connection string. You will need to enter it every time you open DiceBot. Leaving it blank will store your connectionstring in plain text.");
                Console.Write("Password: ");
                string PW = Console.ReadLine();
                DiceBot.PersonalSettings.EncryptConstring = !string.IsNullOrEmpty(PW);
                this.pw = PW;
                DiceBot.PersonalSettings.SetConnectionString(Constring, PW);
                DiceBot.SavePersonalSettings("personalsettings.json");
                DiceBot.LoadPersonalSettings("personalsettings.json");

                return;
            }
        }

        private void PersonalSetting()
        {
            Type strat = (DiceBot.PersonalSettings).GetType();
            PropertyInfo[] props = strat.GetProperties();
            string Out = "";
            while (true)
            {
                int counter = 1;
                foreach (PropertyInfo x in props)
                {
                    if (x.Name != "Stats")
                    {
                        object val = x.GetValue(DiceBot.PersonalSettings);
                        Out += ("(" + counter++ + ")" + x.Name + ": ").PadRight(30) + val?.ToString() + "\r\n";
                    }
                }
                Console.WriteLine(Out + "\r\n\r\n Triggers, done");
                string input = Console.ReadLine();
                if (input.ToLower() == "done")
                {
                    break;
                }
                else if (input.ToLower() == "triggers")
                {

                }
                else
                {

                    int selectedval = 0;
                    if (int.TryParse(input, out selectedval))
                    {
                        if (selectedval > 0 && selectedval <= props.Length)
                        {
                            PropertyInfo x = props[selectedval - 1];
                            if (x.Name != "Stats")
                            {
                                object val = x.GetValue(DiceBot.PersonalSettings);
                                Console.WriteLine("Current Value for {0}: {1}", x.Name, val?.ToString());
                                Console.Write("Enter New Value: ");
                                input = Console.ReadLine();
                                object newval = Convert.ChangeType(input, x.PropertyType);
                                x.SetValue(DiceBot.PersonalSettings, newval);
                                Console.Clear();
                            }
                        }
                    }
                }
            }
        }

        private void BetSettings()
        {
            Type strat = (DiceBot.BetSettings).GetType();
            PropertyInfo[] props = strat.GetProperties();
            string Out = "";
            while (true)
            {
                int counter = 1;
                foreach (PropertyInfo x in props)
                {
                    if (x.Name != "Stats")
                    {
                        object val = x.GetValue(DiceBot.BetSettings);
                        Out += ("(" + counter++ + ")" + x.Name + ": ").PadRight(30) + val?.ToString() + "\r\n";
                    }
                }
                Console.WriteLine(Out + "\r\n\r\n Triggers, done");
                string input = Console.ReadLine();
                if (input.ToLower() == "done")
                {
                    break;
                }
                else if (input.ToLower() == "triggers")
                {
                    BetTriggers();
                }
                else
                {
                    int selectedval = 0;
                    if (int.TryParse(input, out selectedval))
                    {
                        if (selectedval > 0 && selectedval <= props.Length)
                        {
                            PropertyInfo x = props[selectedval - 1];
                            if (x.Name != "Stats")
                            {
                                object val = x.GetValue(DiceBot.BetSettings);
                                Console.WriteLine("Current Value for {0}: {1}", x.Name, val?.ToString());
                                Console.Write("Enter New Value: ");
                                input = Console.ReadLine();
                                object newval = Convert.ChangeType(input, x.PropertyType);
                                x.SetValue(DiceBot.BetSettings, newval);
                                Console.Clear();
                            }
                        }
                    }
                }
            }
        }

        void BetTriggers()
        {
            string input = "";
            do
            {
                int counter = 1;
                foreach (Trigger x in DiceBot.BetSettings.Triggers)
                {
                    Console.WriteLine(string.Format("{4}), {0} when {1} {2} {3}", x.Action, x.TriggerProperty, x.Comparison, x.TargetType == CompareAgainst.Property || x.TargetType == CompareAgainst.Value ? x.Target : "something percentage", counter++));
                }
                Console.WriteLine("Current Actions: Edit (use the item number), New, Delete <item number>, Done");
                input = Console.ReadLine();
                long NumBes = 0;


                if (long.TryParse(input, out NumBes))
                {

                }
                else if (input.ToLower() == "new")
                {
                    Trigger tmp = NewTrigger(true);
                }
                else if (input.ToLower().StartsWith("delete"))
                {
                    if (input.Split(' ').Length > 1)
                    {
                        if (long.TryParse(input.Split(' ')[1], out NumBes))
                        {
                            //remove the applicable trigger from the array nd stuff
                        }
                        else
                        {
                            Console.WriteLine("Enter the number of the trigger you want to delete after the word delete, for example: delete 69");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Enter the number of the trigger you want to delete after the word delete, for example: delete 69");
                    }
                }

            } while (input.ToLower() != "done");
        }

        Trigger NewTrigger(bool BetTrigger)
        {
            Trigger tmp = new Trigger {
                Action = TriggerAction.Stop,
                Comparison = TriggerComparison.LargerThan,
                Enabled =false,
                 
                 
            };

            return tmp;
        }

        Random Rand = new Random();
        void Simulate()
        {
            Console.Write("Number of bets to simulate: ");
            string numbets = Console.ReadLine();
            long NumBes = 0;
            if (long.TryParse(numbets, out NumBes))
            {
                Console.Write("Starting Balance: ");
                string Balance = Console.ReadLine();
                decimal baln = 0;
                if (decimal.TryParse(Balance, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out baln))
                {
                    SimTimer.Start(); Simulation tmp = new Simulation(baln, NumBes,
                        DiceBot.CurrentSite,
                        DiceBot.Strategy,
                        DiceBot.BetSettings,
                        "",
                        false);
                    tmp.OnSimulationWriting += Tmp_OnSimulationWriting;
                    tmp.OnSimulationComplete += Tmp_OnSimulationComplete;
                    tmp.Start();
                }
            }
        }

        private void DiceBot_OnSiteStatsUpdated(object sender, StatsUpdatedEventArgs e)
        {

            //throw new NotImplementedException();
        }

        private void DiceBot_OnSiteRegisterFinished(object sender, GenericEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void DiceBot_OnSiteNotify(object sender, GenericEventArgs e)
        {
            Console.WriteLine(e.Message);
            //throw new NotImplementedException();
        }

        private void DiceBot_OnSiteLoginFinished(object sender, LoginFinishedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void DiceBot_OnSiteError(object sender, DoormatCore.Sites.ErrorEventArgs e)
        {
            Console.Write(e.Message);
            //throw new NotImplementedException();
        }

        private void DiceBot_OnSiteChat(object sender, GenericEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void DiceBot_OnSiteAction(object sender, GenericEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Strategy()
        {
            if (DiceBot.Strategy == null)
            {
                //show list of strategies
                //select 1
                Dictionary<string, Type> strats = DiceBot.GetStrats();
                string[] Names = new string[strats.Keys.Count];
                while (true)
                {
                    string output = "Select Strategy:";
                    int counter = 1;
                    foreach (string x in strats.Keys)
                    {
                        Names[counter - 1] = x;
                        output += string.Format("{0}) {1},\t", counter++, x);
                    }
                    Console.WriteLine(output);
                    string input = Console.ReadLine();
                    int inint = 0;
                    if (int.TryParse(input, out inint))
                    {
                        if (inint > 0 && inint <= strats.Count)
                        {

                            DoormatBot.Strategies.BaseStrategy x = Activator.CreateInstance(strats[Names[inint - 1]]) as DoormatBot.Strategies.BaseStrategy;
                            DiceBot.Strategy = x;
                            break;
                        }
                    }
                }
            }
            //            else
            {
                string input = "";
                do
                {
                    Console.WriteLine("Current Strategy: " + (DiceBot.Strategy).GetType().Name);
                    Console.WriteLine("new, show, set, done");

                    input = Console.ReadLine();
                    if (input.ToLower() == "new")
                    {
                        Dictionary<string, Type> strats = DiceBot.GetStrats();
                        List<string> Names = new List<string>();
                        foreach (string x in strats.Keys)
                            Names.Add(x);
                        while (true)
                        {
                            string output = "Select Strategy:\n";
                            int counter = 1;
                            foreach (string x in strats.Keys)
                            {
                                output += string.Format("{0}) {1}\n", counter++, x);
                            }
                            Console.WriteLine(output);
                            input = Console.ReadLine();
                            int inint = 0;
                            if (int.TryParse(input, out inint))
                            {
                                if (inint > 0 && inint <= strats.Count)
                                {
                                    string ClassName = Names[inint - 1];
                                    Type NewType = strats[ClassName];
                                    BaseStrategy x = Activator.CreateInstance(NewType) as BaseStrategy;
                                    DiceBot.Strategy = x;
                                    break;
                                }
                            }
                        }
                    }
                    else if (input.ToLower() == "show")
                    {
                        //show settings for current 
                        Type strat = (DiceBot.Strategy).GetType();
                        PropertyInfo[] props = strat.GetProperties();
                        string Out = "";
                        foreach (PropertyInfo x in props)
                        {
                            if (x.Name != "Stats")
                            {
                                object val = x.GetValue(DiceBot.Strategy);
                                Out += (x.Name + ": ").PadRight(25) + val?.ToString() + "\r\n";
                            }
                        }
                        Console.WriteLine(Out);
                    }
                    else if (input.ToLower() == "set")
                    {
                        Type strat = (DiceBot.Strategy).GetType();
                        PropertyInfo[] props = strat.GetProperties();
                        
                        while (true)
                        {
                            string Out = "";
                            int counter = 1;
                            foreach (PropertyInfo x in props)
                            {
                                if (x.Name != "Stats")
                                {
                                    object val = x.GetValue(DiceBot.Strategy);
                                    Out += ("(" + counter++ + ")" + x.Name + ": ").PadRight(30) + val?.ToString() + "\r\n";
                                }
                            }
                            Console.WriteLine(Out);
                            input = Console.ReadLine();
                            if (input.ToLower() == "done")
                            {
                                break;
                            }
                            else
                            {
                                int selectedval = 0;
                                if (int.TryParse(input, out selectedval))
                                {
                                    if (selectedval > 0 && selectedval <= props.Length)
                                    {
                                        PropertyInfo x = props[selectedval - 1];
                                        if (x.Name != "Stats")
                                        {
                                            object val = x.GetValue(DiceBot.Strategy);
                                            Console.WriteLine("Current Value for {0}: {1}", x.Name, val?.ToString());
                                            Console.Write("Enter New Value: ");
                                            input = Console.ReadLine();
                                            object newval = Convert.ChangeType(input, x.PropertyType);
                                            x.SetValue(DiceBot.Strategy, newval);
                                            Console.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }

                } while (input.ToLower() != "done");
            }

            /*else
            {
                Console.WriteLine("Invalid state for this command.");
                return;
            }*/
        }

        private void PrintSiteStats()
        {
            if (DiceBot.LoggedIn && DiceBot.CurrentSite != null)
            {
                string jsonres = json.JsonSerializer<SiteStats>(DiceBot.CurrentSite.Stats);
                Console.WriteLine(jsonres);
            }
            else
            {
                Console.WriteLine("Invalid state for this command.");
                return;
            }
        }

        private void Stats()
        {
            if (DiceBot.Stats != null)
            {
                string jsonres = json.JsonSerializer<SessionStats>(DiceBot.Stats);
                Console.WriteLine(jsonres);
            }
            else
            {
                Console.WriteLine("Invalid state for this command.");
                return;
            }
        }

        private void Logout()
        {
            if (DiceBot.LoggedIn && DiceBot.CurrentSite != null)
            {
                Console.WriteLine("Logging you out...");
                DiceBot.CurrentSite.Disconnect();
                DiceBot.LoggedIn = false;
                Console.WriteLine("Logout complete.");
            }
            else
            {
                Console.WriteLine("Invalid state for this command.");
                return;
            }
        }

        void PrintCurrentActions()
        {
            string Actions = "Current Available Actions: Site, Strategy, betsettings, personalsettings, ";
            if (DiceBot.CurrentSite != null)
            {
                Actions += "Currency, ";
            }
            if (DiceBot.CurrentSite != null && DiceBot.Strategy != null)
            {
                Actions += "sim, ";
            }
            if (DiceBot.LoggedIn)
            {
                Actions += "Logout, sitestats, stats, ";
                if (DiceBot.Strategy != null)
                {
                    Actions += "start, ";
                }
            }
            else
            {
                if (DiceBot.CurrentSite != null)
                {
                    Actions += "Login, ";
                }
                else
                {

                }
            }
            Actions += "exit";
            Console.WriteLine(Actions);


        }

        void Login()
        {
            if (DiceBot.CurrentSite == null || DiceBot.LoggedIn)
            {
                Console.WriteLine("Invalid state for this command.");
                return;
            }
            string keepasspw = "";
            string keepassnote = "";
            string keepassusername = "";
            if (DiceBot.KeepassOpen)
            {
                Console.Write("Log in using a KeePass Account? (1: Yes; 2: No;): ");
                string res = Console.ReadLine().ToLower();
                if (res=="1" && res=="yes")
                {
                    KPHelper[] kPHelpers = DiceBot.GetAccounts();
                    int i = 0;
                    Console.WriteLine(string.Format("{0}{1}{2}","".PadRight(5),"Title".PadRight(30),"Username".PadRight(30), "URL".PadRight(30)));
                    foreach (KPHelper x in kPHelpers)
                    {
                        Console.WriteLine(string.Format("{0}{1}{2}{3}",(i++ +")").PadRight(5), x.Title.PadRight(30), x.Username.PadRight(30), x.URL.PadRight(30)));

                    }
                    Console.WriteLine("Enter account number to user: ");
                    res = "";
                    int result = -1;
                    while (res.ToLower()!="cancel" && res.ToLower()!="back" && int.TryParse(res,out result))
                    {
                        res= Console.ReadLine();
                    }
                    if (result>=0)
                    {
                        keepassnote = "";
                        keepassusername = kPHelpers[result].Username;
                        keepasspw = DiceBot.GetPw(kPHelpers[result], out keepassnote);
                    }
                }
            }
            List<DoormatCore.Sites.BaseSite.LoginParamValue> LoginVals = new List<DoormatCore.Sites.BaseSite.LoginParamValue>();
            int counter = 0;
            foreach (DoormatCore.Sites.BaseSite.LoginParameter x in DiceBot.CurrentSite.LoginParams)
            {
                string Result = "";
                if (!x.Masked && !string.IsNullOrWhiteSpace(keepassusername) && counter == 0)
                {
                    Result = keepassusername;
                }
                else if (x.Masked && !string.IsNullOrWhiteSpace(keepasspw) && (counter == 0 || counter == 1))
                {
                    Result = keepasspw;
                }
                else if (x.Masked && !string.IsNullOrWhiteSpace(keepassnote) && (counter == 2))
                {
                    Result = keepassnote;
                }
                if (Result== "")
                {
                    do
                    {
                        Console.Write(x.Name + ": ");

                        if (x.Masked)
                        {
                            string pass = "";
                            ConsoleKeyInfo key;
                            do
                            {
                                key = Console.ReadKey(true);

                                if (key.Key != ConsoleKey.Backspace)
                                {
                                    pass += key.KeyChar;
                                    Console.Write("*");
                                }
                                else
                                {
                                    pass = pass.Substring(0, (pass.Length - 1));
                                    Console.Write("\b \b");
                                }
                            }
                            // Stops Receving Keys Once Enter is Pressed
                            while (key.Key != ConsoleKey.Enter);
                            foreach (char y in pass)
                            {
                                Console.Write("\b \b");
                            }
                            if (pass.EndsWith("\r"))
                                pass = pass.Replace("\r", "");
                            Result = pass;
                            Console.WriteLine();
                        }
                        else
                        {
                            Result = Console.ReadLine();
                        }
                    } while (string.IsNullOrEmpty(Result) && x.Required);
                }
                LoginVals.Add(new DoormatCore.Sites.BaseSite.LoginParamValue { Param = x, Value = Result });
            }
            DiceBot.OnSiteLoginFinished -= CurrentSite_LoginFinished;
            DiceBot.OnSiteLoginFinished += CurrentSite_LoginFinished;
            Console.WriteLine("Logging in...");
            DiceBot.Login(LoginVals.ToArray());
        }

        private void CurrentSite_LoginFinished(object sender, DoormatCore.Sites.LoginFinishedEventArgs e)
        {
            Console.WriteLine(e.Success ? "Logged in!" : "Failed to log in to " + DiceBot.CurrentSite.SiteName);
            if (e.Success)
            {
                Console.WriteLine(json.JsonSerializer<SiteStats>(e.Stats));
                Console.WriteLine("Current Actions: logout, site, currency, strategy, stats, start, sim");
            }
        }

        private void Tmp_OnSimulationComplete(object sender, EventArgs e)
        {
            SimTimer.Stop();
            Simulation tmp = sender as Simulation;

            Console.WriteLine(string.Format("Simulation Complete: {0} bets simulated in {1} Seconds", tmp.TotalBetsPlaced, SimTimer.ElapsedMilliseconds / 1000.0));
            Console.WriteLine(string.Format("Ending Balance: {0}. Profit:{5}. Wins {1}. Losses:{2}. Longest Winning Streak: {3}. Longest Losing Streak: {4}.",
                tmp.Balance, tmp.Stats.Wins, tmp.Stats.Losses, tmp.Stats.BestStreak, tmp.Stats.WorstStreak, tmp.Profit));
            SimTimer.Reset();
        }

        private void Tmp_OnSimulationWriting(object sender, EventArgs e)
        {
            Simulation tmp = sender as Simulation;
            Console.WriteLine("Simulation Progress: " + tmp.TotalBetsPlaced + " bets of " + tmp.Bets);
            if (tmp.TotalBetsPlaced > 0)
            {
                long ElapsedMilliseconds = SimTimer.ElapsedMilliseconds;
                decimal Progress = (decimal)tmp.TotalBetsPlaced / (decimal)tmp.Bets;
                decimal totaltime = ElapsedMilliseconds / Progress;

                Console.WriteLine("Progress: {3:p}, Projected Runtime: {0}, Run Time: {1}, Projected Remaining: {2}", TimeString((long)totaltime), TimeString((long)ElapsedMilliseconds), TimeString((long)totaltime - ElapsedMilliseconds), Progress);

                if (tmp.TotalBetsPlaced % (1000000) == 0)
                {
                    Console.WriteLine(string.Format("Ending Balance: {0}. Profit:{5}. Wins {1}. Losses:{2}. Longest Winning Streak: {3}. Longest Losing Streak: {4}.",
                tmp.Balance, tmp.Stats.Wins, tmp.Stats.Losses, tmp.Stats.BestStreak, tmp.Stats.WorstStreak, tmp.Profit));

                }
            }
        }

        private string TimeString(long Milliseconds)
        {
            decimal remaining = Milliseconds;
            decimal days = Math.Floor(remaining / (24 * 60 * 60 * 1000));
            remaining = remaining % ((24 * 60 * 60 * 1000));
            decimal hours = Math.Floor(remaining / (60 * 60 * 1000));
            remaining = remaining % ((60 * 60 * 1000));
            decimal Minutes = Math.Floor(remaining / (60 * 1000));
            remaining = remaining % ((60 * 1000));
            decimal Seconds = Math.Floor(remaining / (1000));
            remaining = remaining % ((1000));

            return (days > 0 ? days + "d " : "") + hours.ToString("00") + ":" + Minutes.ToString("00") + ":" + Seconds.ToString("00") + "." + remaining.ToString("0000");
        }

        private void DiceBot_OnSiteDiceBetFinished(object sender, DoormatCore.Sites.BetFinisedEventArgs e)
        {
            
            Console.WriteLine(DoormatCore.Helpers.json.JsonSerializer<DiceBet>(e.NewBet as DiceBet));
            if (DiceBot.Stats.Bets % 100 == 0)
            {
                Console.WriteLine("\r\n\r\n");
                Console.WriteLine(DoormatCore.Helpers.json.JsonSerializer<SessionStats>(DiceBot.Stats));
                Console.WriteLine("\r\n\r\n");
            }
        }

        void SelectSite()
        {

            DiceBot.CompileSites();
            string Sites = "";
            int counter = 1;
            foreach (SitesList x in DiceBot.Sites)
            {
                Sites += string.Format("{0}) {1}\n", counter++, x.Name);
            }
            while (true)
            {
                Console.WriteLine("Which site would you like to use? Enter the correspondiong number.");
                Console.WriteLine(Sites);
                string input = Console.ReadLine();
                int selectedval = 0;
                if (int.TryParse(input, out selectedval))
                {
                    if (selectedval > 0 && selectedval <= DiceBot.Sites.Count)
                    {
                        SitesList selected = DiceBot.Sites[selectedval - 1];
                        DiceBot.CurrentSite = Activator.CreateInstance(selected.SiteType()) as DoormatCore.Sites.BaseSite;


                        SelectCurrency();
                        break;
                    }
                }
            }
        }



        void SelectCurrency()
        {
            if (DiceBot.CurrentSite == null)
            {
                Console.WriteLine("Invalid state for this command.");
                return;
            }
            if (DiceBot.CurrentSite != null)
            {
                if (DiceBot.CurrentSite.Currencies.Length == 1)
                {
                    Console.WriteLine("Using " + DiceBot.CurrentSite.Currencies[0]);
                }
                else
                {
                    string Currs = "";
                    int counter = 1;
                    foreach (var x in DiceBot.CurrentSite.Currencies)
                    {
                        Currs += string.Format("{0}) {1}\n", counter++, x);
                    }
                    while (true)
                    {
                        Console.WriteLine("Currency:");
                        Console.WriteLine(Currs);
                        string input = Console.ReadLine();
                        int selectedval = 0;
                        if (int.TryParse(input, out selectedval))
                        {
                            if (selectedval > 0 && selectedval <= DiceBot.CurrentSite.Currencies.Length)
                            {
                                DiceBot.CurrentSite.Currency = selectedval - 1;
                                Console.WriteLine("Using " + DiceBot.CurrentSite.Currencies[selectedval - 1]);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
