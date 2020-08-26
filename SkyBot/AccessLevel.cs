using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot
{
    public enum AccessLevel : int
    {
        User,
        VIP,
        Moderator,
        Admin,
        /// <summary>
        /// Owner of the guild will always be Host
        /// </summary>
        Host,
        /// <summary>
        /// Only for bot developer, use to manage the bot or help if problems appear
        /// </summary>
        Dev
    }
}
