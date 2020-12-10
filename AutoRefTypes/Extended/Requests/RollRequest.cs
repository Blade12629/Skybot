using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoRefTypes.Extended.Requests
{
    /// <summary>
    /// Used to request rolls
    /// </summary>
    public class RollRequest : ChatRequest
    {
        /// <summary>
        /// Rolled value
        /// </summary>
        public long Rolled { get; private set; }
        /// <summary>
        /// Min roll value
        /// </summary>
        public long Min { get; private set; }
        /// <summary>
        /// Max roll value
        /// </summary>
        public long Max { get; private set; }

        /// <summary>
        /// IRC username
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Message to send after calling <see cref="ChatRequest.Request"/>, leave empty or null to not send
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Used to request rolls
        /// </summary>
        /// <param name="user">IRC username</param>
        /// <param name="message">Message to send after calling <see cref="ChatRequest.Request"/>, leave empty or null to not send</param>
        public RollRequest(ILobby lobby, string user, string message = null) : base(lobby)
        {
            Message = message;
            User = user;
            Min = 0;
            Max = 100;
        }

        /// <summary>
        /// Checks for !roll
        /// </summary>
        public override bool CheckStringCondition(IChatMessage msg)
        {
            if (msg.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase) &&
                msg.Message.StartsWith($"{User.Replace('_', ' ')} rolls ", StringComparison.CurrentCultureIgnoreCase))
                return true;
            else if (msg.From.Equals(User, StringComparison.CurrentCultureIgnoreCase) &&
                     msg.Message.StartsWith("!roll", StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }

        public override void Trigger(IChatMessage msg)
        {
            string[] split = msg.Message.Split(' ');

            if (msg.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
            {
                string rolledStr = split[split.Length - 2];
                Rolled = long.Parse(rolledStr);
                RequestFinished = true;
            }
            else
            {
                if (split.Length >= 3)
                {
                    if (long.TryParse(split[1], out long min))
                    {
                        Min = min;

                        if (long.TryParse(split[2], out long max))
                            Max = max;
                    }
                    else if (long.TryParse(split[2], out long max))
                        Max = max;
                }
                else if (split.Length >= 2)
                {
                    if (long.TryParse(split[1], out long max))
                        Max = max;
                }
            }
        }

        protected override void OnAfterRequest()
        {
            if (!string.IsNullOrEmpty(Message))
                _lobby.SendChannelMessage(Message);
        }
    }
}
