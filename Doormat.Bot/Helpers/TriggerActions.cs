using Gambler.Bot.AutoBet.Helpers;
using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gambler.Bot.Core.Helpers
{
    public enum TriggerAction { Alarm, Chime, Email, Popup, Stop, Reset, Withdraw, Tip, Invest, Bank, ResetSeed, Switch }
    public enum CompareAgainst { Value, Percentage, Property }
    public enum TriggerComparison { Equals, LargerThan, SmallerThan, LargerOrEqualTo, SmallerOrEqualTo, Modulus }
    public class Trigger:INotifyPropertyChanged
    {
        TriggerAction action;
        public TriggerAction Action { get=>action; set { action = value; RaisePropertyChanged(); } }
        bool enabled;
        public bool Enabled { get=>enabled; set { enabled = value; RaisePropertyChanged(); } }
        string triggerProperty;
        public string TriggerProperty { get=>triggerProperty; set { triggerProperty = value; RaisePropertyChanged(); } }
        CompareAgainst targetType;
        public CompareAgainst TargetType { get=> targetType; set { targetType = value; RaisePropertyChanged(); } }
        string target;
        public string Target { get => target; set { target = value; RaisePropertyChanged(); } }
        TriggerComparison comparison;
        public TriggerComparison Comparison { get => comparison; set { comparison = value; RaisePropertyChanged(); } }
        decimal percentage;
        public decimal Percentage { get => percentage; set { percentage = value; RaisePropertyChanged(); } }
        CompareAgainst valueType;
        public CompareAgainst ValueType { get => valueType; set { valueType = value; RaisePropertyChanged(); } }
        string valueProperty;
        public string ValueProperty { get => valueProperty; set { valueProperty = value; RaisePropertyChanged(); } }
        decimal valueValue;
        public decimal ValueValue { get => valueValue; set { valueValue = value; RaisePropertyChanged(); } }
        string destination;
        public string Destination { get => destination; set { destination = value; RaisePropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (propertyName is not null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        PropertyInfo triggerPropertyInfo;
        PropertyInfo targetPropertyInfo;
        PropertyInfo ValuePropertyInfo;

        public bool CheckNotification(SessionStats Stats, SiteStats siteStats)
        {

            
            decimal Source = 0;
            try
            {
               
                Source=getValue(TriggerProperty, triggerPropertyInfo, Stats, siteStats);
            }
            catch
            {
                throw new Exception("Invalid Trigger Field");
            }
            if (TargetType == CompareAgainst.Value)
            {
                decimal TargetValue = 0;
                if (!decimal.TryParse(Target, System.Globalization.NumberStyles.Number, System.Globalization.NumberFormatInfo.InvariantInfo, out TargetValue))
                {
                    throw new Exception("Invalid Target Value");
                }
                return DoComparison(Comparison, Source, TargetValue);
                
            }
            else if (TargetType == CompareAgainst.Percentage)
            {
                decimal TargetValue = 0;
                try
                {
                    TargetValue = getValue(Target, targetPropertyInfo, Stats, siteStats);

                }
                catch
                {
                    throw new Exception("Invalid Target Property");
                }
                return DoComparison(Comparison, (Source / TargetValue) * 100m, Percentage);
                
            }
            else if (TargetType == CompareAgainst.Property)
            {
                decimal TargetValue = 0;
                try
                {

                    TargetValue = getValue(Target, targetPropertyInfo, Stats, siteStats);
                }
                catch
                {
                    throw new Exception("Invalid Target Property");
                }
                return DoComparison(Comparison, (Source), TargetValue);

            }

            return false;
        }

        decimal getValue(string PropertyName, PropertyInfo prop, SessionStats session, SiteStats site)
        {
            string[] parts = PropertyName.Split('.');
            bool useSession = (parts[0] == nameof(SessionStats));
            if (prop?.Name != parts[1])
            {
                Type type = null;
                if (useSession)
                {
                    type = session.GetType();
                }
                else
                {
                    type = site.GetType();
                }
                triggerPropertyInfo = type.GetProperty(parts[1]);
            }
            object result = triggerPropertyInfo.GetValue(useSession ? session : site);

            if (result is int iresult)
                return (decimal)iresult;
            else if (result is long lresult)
                return (decimal)lresult;
            else if (result is decimal dresult)
                return dresult;
            else
                return 0;
        }

        bool DoComparison(TriggerComparison Comparison, decimal Source, decimal TargetValue)
        {
            switch (Comparison)
            {
                case TriggerComparison.Equals: return Source == TargetValue;
                case TriggerComparison.LargerThan: return Source > TargetValue;
                case TriggerComparison.SmallerThan: return Source < TargetValue;
                case TriggerComparison.LargerOrEqualTo: return Source >= TargetValue;
                case TriggerComparison.SmallerOrEqualTo: return Source <= TargetValue;
                case TriggerComparison.Modulus: return Source % TargetValue == 0;
            }
            return false;
        }

        public decimal GetValue(SessionStats Stats, SiteStats stats)
        {
            
            

            decimal Source = 0;
            try
            {
                Source = getValue(ValueProperty, ValuePropertyInfo, Stats, stats);
            }
            catch
            {
                throw new Exception("Invalid Value Field");
            }
            if (ValueType == CompareAgainst.Percentage)
            {
                return Source * (ValueValue / 100m);
            }
            else if (ValueType == CompareAgainst.Value)
            {
                return (ValueValue);
            }
            else if (ValueType == CompareAgainst.Property)
            {
                return Source;
            }
            return 0;
        }
        [System.Text.Json.Serialization.JsonIgnore]
        public string TriggerDescription { get => ToString(); }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("When ");
            sb.Append(TriggerProperty);
            sb.Append(" ");
            sb.Append(Comparison.ToString());
            sb.Append(" ");
            if (TargetType == CompareAgainst.Value)
            {
                sb.Append(Target);
            }
            else if (TargetType == CompareAgainst.Percentage)
            {
                sb.Append(Percentage);
                sb.Append("% of ");
                sb.Append(Target);
            }
            else if (TargetType == CompareAgainst.Property)
            {
                sb.Append(Target);
                
            }
            sb.Append(" ");

            ToStringAction(sb);


            return sb.ToString();
        }
        void ToStringAction(StringBuilder sb)
        {
            string valuestring = "";
            switch (ValueType)
            {
                case CompareAgainst.Value: valuestring = ValueValue.ToString(); break;
                case CompareAgainst.Percentage: valuestring = ValueValue.ToString() + "% of " +ValueProperty; break;
                case CompareAgainst.Property: valuestring = ValueProperty; break;
            }
            

            switch (Action)
            {
                case TriggerAction.Alarm: sb.Append("play an alarm"); break;
                case TriggerAction.Bank: sb.Append("bank " + valuestring); break;
                case TriggerAction.Chime: sb.Append("play a chime"); break;
                case TriggerAction.Email: sb.Append("send an email to "+Destination); break;
                case TriggerAction.Invest: sb.Append("invest " + valuestring ); break;
                case TriggerAction.Popup: sb.Append("show a notification"); break;
                case TriggerAction.Reset: sb.Append("reset"); break;
                case TriggerAction.ResetSeed: sb.Append("reset seed"); break;
                case TriggerAction.Stop: sb.Append("stop betting"); break;
                case TriggerAction.Switch: sb.Append("switch side"); break;
                case TriggerAction.Tip: sb.Append("send a tip of " + valuestring + " to " + Destination); break;
                case TriggerAction.Withdraw: sb.Append("withdraw "+ valuestring +" to "+Destination); break;
            }
        }
    }
    public class NotificationEventArgs:EventArgs
    {
        public Trigger NotificationTrigger { get; set; }
    }
}
