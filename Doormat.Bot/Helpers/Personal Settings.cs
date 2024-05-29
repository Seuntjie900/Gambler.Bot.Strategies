using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using static Gambler.Bot.AutoBet.Helpers.PersonalSettings;
using Gambler.Bot.Core.Helpers;
using Doormat.Bot.Helpers;
using Gambler.Bot.Core.Enums;

namespace Gambler.Bot.AutoBet.Helpers
{
    public class PersonalSettings
    {
        private List<Trigger> _triggers = new List<Trigger>();
        private Dictionary<ErrorType, ErrorSetting> _Errors = new Dictionary<ErrorType, ErrorSetting>();

        public List<Trigger> Notifications { get { return _triggers; } private set { _triggers = value; } }
        private List<ErrorSetting> errorSettings;

        public List<ErrorSetting> ErrorSettings
        {
            get { return ArrayFromDictonary(); }
            set {  CompareErrorDictionary(value); }
        }

        private List<ErrorSetting> ArrayFromDictonary()
        {
            List<ErrorSetting> settings = new List<ErrorSetting>();
            foreach (ErrorType x in _Errors.Keys)
            {
                settings.Add(_Errors[x]);
            }
            return settings;
        }

        private void CompareErrorDictionary(List<ErrorSetting> newSettings)
        {
            foreach (ErrorSetting x in newSettings)
            {
                if (_Errors.ContainsKey(x.Type))
                {
                    _Errors[x.Type] = x;
                }
                else
                {
                    _Errors.Add(x.Type, x);
                }
            }
        }


        #region Error Settings
        //betting error
        //Withdrawal error
        //Invest error
        //Tip Error
        //Reset Seed Error
        /*public ErrorActions BetError { get; set; } = ErrorActions.Retry;
        public ErrorActions BalanceTooLow { get; set; } = ErrorActions.Stop;
        public ErrorActions WithdrawalError { get; set; } = ErrorActions.Resume;
        public ErrorActions InvestError { get; set; } = ErrorActions.Resume;
        public ErrorActions TipError { get; set; } = ErrorActions.Resume;
        public ErrorActions ResetSeedError { get; set; } = ErrorActions.Resume;*/
        public int RetryDelay { get; set; } = 30;
        public int RetryAttempts { get; set; } = 0;

        #endregion

        public string EncrConnectionString { get; set; }

        public string Provider { get; set; }

        public bool EncryptConstring { get; set; }

        
        public string GetConnectionString(string Password)
        {
            if (EncryptConstring)
            {
                return EncryptionHelper.Decrypt(EncrConnectionString, Password);
            }
            else
            {
                return EncrConnectionString;
            }
        }

        public void SetConnectionString(string ConnectionString, string Password)
        {
            if (EncryptConstring)
            {
                EncrConnectionString = EncryptionHelper.Encrypt(ConnectionString, Password);
            }
            else
            {
                EncrConnectionString = ConnectionString;
            }
        }

        public ErrorSetting GetErrorSetting(ErrorType Type)
        {
            if (_Errors.ContainsKey(Type))
                return _Errors[Type];
            return null;
        }

        public class ErrorSetting
        {
            public ErrorType Type { get; set; }
            public ErrorActions Action { get; set; }
        }
        public class GetConstringPWEventArgs:EventArgs
        {
            public string Password { get; set; }
        }

        public string KeepassDatabase { get; set; }

        public static PersonalSettings Default()
        {
            PersonalSettings settings = new PersonalSettings();
            settings.EncryptConstring = false;
            settings.Provider = "SQLite";
            settings.EncrConnectionString = string.Format("Data Source={0};Version=3;Compress=True;", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KryGamesBot\\KryGamesBot.db");
            settings.RetryAttempts = 5;
            settings.RetryDelay = 30;
            PersonalSettings.ErrorSetting[] tmp = new PersonalSettings.ErrorSetting[Enum.GetNames(typeof(ErrorType)).Length];
            tmp[0] = new PersonalSettings.ErrorSetting { Type = ErrorType.BalanceTooLow, Action = ErrorActions.Retry };
            tmp[1] = new PersonalSettings.ErrorSetting { Type = ErrorType.BetMismatch, Action = ErrorActions.Stop };
            tmp[2] = new PersonalSettings.ErrorSetting { Type = ErrorType.InvalidBet, Action = ErrorActions.Stop };
            tmp[3] = new PersonalSettings.ErrorSetting { Type = ErrorType.NotImplemented, Action = ErrorActions.Stop };
            tmp[4] = new PersonalSettings.ErrorSetting { Type = ErrorType.Other, Action = ErrorActions.Stop };
            tmp[5] = new PersonalSettings.ErrorSetting { Type = ErrorType.ResetSeed, Action = ErrorActions.Resume };
            tmp[6] = new PersonalSettings.ErrorSetting { Type = ErrorType.Tip, Action = ErrorActions.Resume };
            tmp[7] = new PersonalSettings.ErrorSetting { Type = ErrorType.Unknown, Action = ErrorActions.Stop };
            tmp[8] = new PersonalSettings.ErrorSetting { Type = ErrorType.Withdrawal, Action = ErrorActions.Resume };
            tmp[9] = new PersonalSettings.ErrorSetting { Type = ErrorType.BetTooLow, Action = ErrorActions.Stop };
            settings.ErrorSettings = new List<ErrorSetting>(tmp);
            
            return settings;
        }
    }
   
}
