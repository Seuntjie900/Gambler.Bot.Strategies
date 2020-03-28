using DoormatBot;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DoormatControllers
{
    /// <summary>
    /// At the EDGE of the DOORMAT is the first porch step. Get it? It's the interface to handle communication between an edge-js 
    /// application for a front end and doormat.
    /// </summary>
    public class PorchStep
    {
        static Doormat Instance = null;
        Dictionary<string, Func<object, Task<object>>> Callbacks;
        public int counter { get; set; } = 0;
        public Task<object> CreateInstance(IDictionary<string, object> input)
        {
            using (StreamWriter x = File.AppendText("test.txt"))
            {
                x.WriteLine(input);
                foreach (PropertyInfo y in input.GetType().GetProperties())
                {
                    x.WriteLine(y.Name);
                }
                x.WriteLine(input["OnGameChanged"]);
                
                
            }
            if (Instance == null)
            {
                Logger.LoggingLevel = 6;
                Instance = new Doormat();
                Callbacks = new Dictionary<string, Func<object, Task<object>>>();
                Instance.OnGameChanged += Instance_OnGameChanged;
                Callbacks.Add("OnGameChanged", (Func<object, Task<object>>)input["OnGameChanged"]);
                Instance.OnSiteAction += Instance_OnSiteAction;
                Callbacks.Add("OnSiteAction", (Func<object, Task<object>>)input["OnSiteAction"]);
                Instance.OnSiteChat += Instance_OnSiteChat;
                Callbacks.Add("OnSiteChat", (Func<object, Task<object>>)input["OnSiteChat"]);
                Instance.OnSiteBetFinished += Instance_OnSiteDiceBetFinished;
                Callbacks.Add("OnSiteDiceBetFinished", (Func<object, Task<object>>)input["OnSiteDiceBetFinished"]);
                Instance.OnSiteError += Instance_OnSiteError;
                Callbacks.Add("OnSiteError", (Func<object, Task<object>>)input["OnSiteError"]);
                Instance.OnSiteLoginFinished += Instance_OnSiteLoginFinished;
                Callbacks.Add("OnSiteLoginFinished", (Func<object, Task<object>>)input["OnSiteLoginFinished"]);
                Instance.OnSiteNotify += Instance_OnSiteNotify;
                Callbacks.Add("OnSiteNotify", (Func<object, Task<object>>)input["OnSiteNotify"]);
                Instance.OnSiteRegisterFinished += Instance_OnSiteRegisterFinished;
                Callbacks.Add("OnSiteRegisterFinished", (Func<object, Task<object>>)input["OnSiteRegisterFinished"]);
                Instance.OnSiteStatsUpdated += Instance_OnSiteStatsUpdated;
                Callbacks.Add("OnSiteStatsUpdated", (Func<object, Task<object>>)input["OnSiteStatsUpdated"]);
                Instance.LoadPersonalSettings("personalsettings.json");
                Instance.LoadBetSettings("betsettings.json");
                Instance.CompileSites();
            }
            //Instance.Sites = null;
            Logger.DumpLog("Finished creating instance, creating task and returning",6);
            Task<object> taskA = new Task<object>(() => (Instance));
            //taskA.Start();
            taskA.RunSynchronously();
            Logger.DumpLog("Finished creating instance, returning", 6);
            return taskA;
        }
        public Task<object> GetInstanceInfo(dynamic value)
        {
            Task<object> taskA = new Task<object>(() => (Instance));
            //taskA.Start();
            taskA.RunSynchronously();
            return taskA;
        }
        public Task<object> SetSite(IDictionary<string, object> input)
        {
            Task<object> taskA = new Task<object>(() => SetSite((string)input["SiteName"]));
                        //taskA.Start();
            taskA.Start();
            return taskA;
        }

        private object SetSite(string Name)
        {
            foreach (SitesList x in Instance.Sites)
            {
                if (x.Name == Name)
                {
                    Instance.CurrentSite = Activator.CreateInstance(x.SiteType()) as DoormatCore.Sites.BaseSite;
                    return true;
                }
            }
            return false;
        }

        public Task<object> LogIn(IDictionary<string, object> input)
        {
            Task<object> taskA = new Task<object>(() => logIn(input));
            //taskA.Start();
            taskA.Start();
            return taskA;
        }
        private object logIn(IDictionary<string, object> input)
        {
            /*if (input.ContainsKey("account"))
            {
                var Accounts = Instance.GetAccountsForCurrentSite();
                foreach (var x in Accounts)
                {
                    if (x.AccountName== (string)input["account"])
                    {
                        Instance.Login(x);
                        return true;
                    }
                }
            }
            else*/
            {
                List<BaseSite.LoginParamValue> Value = new List<BaseSite.LoginParamValue>();
                foreach (var x in Instance.CurrentSite.LoginParams)
                {
                    Value.Add(new BaseSite.LoginParamValue { Param = x, Value = (string)input[x.Name.Replace(" ","_")] });

                }
                Instance.Login(Value.ToArray());
                return true;
            }
            return false;
        }

        private void Instance_OnSiteStatsUpdated(object sender, DoormatCore.Sites.StatsUpdatedEventArgs e)
        {
            Callbacks["OnSiteStatsUpdated"](e);
        }

        private void Instance_OnSiteRegisterFinished(object sender, DoormatCore.Sites.GenericEventArgs e)
        {
            Callbacks["OnSiteRegisterFinished"](e);
        }

        private void Instance_OnSiteNotify(object sender, DoormatCore.Sites.GenericEventArgs e)
        {
            Callbacks["OnSiteNotify"](e);
        }

        private void Instance_OnSiteLoginFinished(object sender, DoormatCore.Sites.LoginFinishedEventArgs e)
        {
            Callbacks["OnSiteLoginFinished"](e);
        }

        private void Instance_OnSiteError(object sender, DoormatCore.Sites.ErrorEventArgs e)
        {
            Callbacks["OnSiteError"](e);
        }

        private void Instance_OnSiteDiceBetFinished(object sender, DoormatCore.Sites.BetFinisedEventArgs e)
        {
            Callbacks["OnSiteDiceBetFinished"](e);
        }

        private void Instance_OnSiteChat(object sender, DoormatCore.Sites.GenericEventArgs e)
        {
            Callbacks["OnSiteChat"](e);

        }

        private void Instance_OnSiteAction(object sender, DoormatCore.Sites.GenericEventArgs e)
        {
            Callbacks["OnSiteAction"](e);
        }

        private void Instance_OnGameChanged(object sender, EventArgs e)
        {
            Callbacks["OnGameChanged"](e);
        }

        void CallFunc()
        {
            DoSomething(true, 1, somethingsbeingdone);
            DoSomething(true, 1, somethingsbeingdoneAgain);
        }
        
        void funkycall()
        {
            
        }

        private void DoSomething(bool x, int y, Func<bool, int, string, bool> funcc)
        {
            bool result = funcc(x, y, "do something");
                        
        }

        bool somethingsbeingdone(bool x, int y, string varp)
        {
            return (y.ToString() == varp) == x;
        }
        bool somethingsbeingdoneAgain(bool x, int y, string varp)
        {
            return (y.ToString() == varp) != x;
        }
    }
}
