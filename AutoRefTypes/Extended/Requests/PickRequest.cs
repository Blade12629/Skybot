using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes.Extended.Requests
{
    /// <summary>
    /// Used to request picks or bans
    /// </summary>
    public class PickRequest : ChatRequest
    {
        /// <summary>
        /// IRC Username
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// True - Ban request, False - Pick request
        /// </summary>
        public bool IsBanRequest { get; set; }
        /// <summary>
        /// Message to send after calling <see cref="ChatRequest.Request"/>, leave empty or null to not send
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Map that has been picked by the user
        /// </summary>
        public ulong MapPick { get; private set; }
        /// <summary>
        /// Is the pick valid? Invalid if parsing the mapid fails
        /// </summary>
        public bool InvalidPick { get; private set; }

        /// <summary>
        /// Used to request picks or bans
        /// </summary>
        /// <param name="user">IRC Username</param>
        /// <param name="isBanRequest">True - Ban request, False - Pick request</param>
        /// <param name="message">Message to send after calling <see cref="ChatRequest.Request"/>, leave empty or null to not send</param>
        public PickRequest(ILobby lobby, string user, bool isBanRequest, string message) : base(lobby)
        {
            User = user;
            IsBanRequest = isBanRequest;
            Message = message;
        }

        /// <summary>
        /// Checks for !pick mapId if <see cref="IsBanRequest"/> == false or checks for !ban mapId if <see cref="IsBanRequest"/> == true
        /// </summary>
        public override bool CheckStringCondition(IChatMessage msg)
        {
            if (!msg.From.Equals(User, StringComparison.CurrentCultureIgnoreCase))
                return false;

            if ((IsBanRequest && msg.From.StartsWith("!ban", StringComparison.CurrentCultureIgnoreCase)) ||
                (!IsBanRequest && msg.From.StartsWith("!pick", StringComparison.CurrentCultureIgnoreCase)))
                return true;

            return false;
        }

        public override void Trigger(IChatMessage msg)
        {
            string mapStr = msg.Message.Split(' ')[1];

            if (!ulong.TryParse(mapStr, out ulong mapPick))
                InvalidPick = true;

            MapPick = mapPick;
        }

        protected override void OnAfterRequest()
        {
            if (!string.IsNullOrEmpty(Message))
                _lobby.SendChannelMessage(Message);
        }
    }
}
