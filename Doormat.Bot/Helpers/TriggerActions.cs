using Amazon.SecurityToken.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatCore.Helpers
{
    public enum TriggerAction { Alarm, Chime, Email, Popup, Stop, Reset, Withdraw, Tip, Invest, Bank, ResetSeed, Switch }
    public enum CompareAgainst { Value, Percentage, Property }
    public enum TriggerComparison { Equals, LargerThan, SmallerThan, LargerOrEqualTo, SmallerOrEqualTo, Modulus }
    public class Trigger
    {
        public TriggerAction Action { get; set; }
        public bool Enabled { get; set; }
        public string TriggerProperty { get; set; }
        public CompareAgainst TargetType { get; set; }
        public string Target { get; set; }
        public TriggerComparison Comparison { get; set; }
        public decimal Percentage { get; set; }
        public CompareAgainst ValueType { get; set; }
        public string ValueProperty { get; set; }
        public decimal ValueValue { get; set; }
        public string Destination { get; set; }

        public bool CheckNotification(object Stats)
        {
            Type StatsType = Stats.GetType();

            decimal Source = 0;
            try
            {
                Source = (decimal)(StatsType.GetProperty(TriggerProperty).GetValue(Stats));
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
                    TargetValue = (decimal)(StatsType.GetProperty(Target).GetValue(Stats));
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
                    TargetValue = (decimal)(StatsType.GetProperty(Target).GetValue(Stats));
                }
                catch
                {
                    throw new Exception("Invalid Target Property");
                }
                return DoComparison(Comparison, (Source), TargetValue);

            }

            return false;
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

        public decimal GetValue(object Stats)
        {
            Type StatsType = Stats.GetType();

            decimal Source = 0;
            try
            {
                Source = (decimal)(StatsType.GetProperty(ValueProperty).GetValue(Stats));
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
