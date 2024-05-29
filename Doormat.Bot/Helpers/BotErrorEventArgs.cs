using Gambler.Bot.Core.Events;

namespace Doormat.Bot.Helpers
{
    public enum ErrorActions { ResumeAsWin, ResumeAsLoss, Resume, Stop, Reset, Retry }
    public class BotErrorEventArgs:ErrorEventArgs
    {
        public ErrorActions Action { get; set; }
        public BotErrorEventArgs(ErrorEventArgs args):base()
        {
            Action = ErrorActions.Retry;
            Message = args.Message;
            Type = args.Type;
            this.Handled = args.Handled;
            this.Fatal = args.Fatal;            
        }
    }
}
