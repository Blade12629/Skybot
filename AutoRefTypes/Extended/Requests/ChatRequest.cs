using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes.Extended.Requests
{
    /// <summary>
    /// Used to request chat responses
    /// </summary>
    public abstract class ChatRequest
    {
        /// <summary>
        /// Request has been finished
        /// </summary>
        public bool RequestFinished { get; protected set; }
        /// <summary>
        /// Request was cancelled
        /// </summary>
        public bool RequestCancelled { get; private set; }
        /// <summary>
        /// Request is currently waiting for response
        /// </summary>
        public bool IsRequested { get; private set; }

        protected readonly ILobby _lobby;

        /// <summary>
        /// Used to request chat responses
        /// </summary>
        protected ChatRequest(ILobby lobby)
        {
            _lobby = lobby;
        }

        /// <summary>
        /// Starts the request
        /// </summary>
        public void Request()
        {
            if (IsRequested)
                return;

            RequestFinished = false;
            RequestCancelled = false;

            _lobby.RegisterRequest(this);

            IsRequested = true;

            OnAfterRequest();
        }

        /// <summary>
        /// Cancels a running request
        /// </summary>
        public void CancelRequest()
        {
            if (RequestFinished || RequestCancelled || !IsRequested)
                return;

            RequestCancelled = true;
            IsRequested = false;
        }

        /// <summary>
        /// Called when a request finishes
        /// </summary>
        public void OnFinishRequest()
        {
            RequestFinished = true;
            IsRequested = false;
        }

        /// <summary>
        /// Called after a request has been started
        /// </summary>
        protected virtual void OnAfterRequest()
        {

        }

        /// <summary>
        /// Checks if the request should be triggered
        /// </summary>
        /// <returns>if true calls <see cref="Trigger(IChatMessage)"/></returns>
        public abstract bool CheckStringCondition(IChatMessage msg);

        /// <summary>
        /// Triggers the request
        /// </summary>
        public abstract void Trigger(IChatMessage msg);
    }
}
