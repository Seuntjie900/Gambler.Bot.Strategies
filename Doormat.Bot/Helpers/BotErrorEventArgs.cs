using Gambler.Bot.Core.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
