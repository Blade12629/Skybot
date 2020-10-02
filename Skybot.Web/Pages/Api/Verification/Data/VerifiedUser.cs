using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skybot.Web.Pages.Api.Verification.Data
{
    public struct VerifiedUser
    {
        public long OsuId { get; }
        public long DiscordUserId { get; }

        public VerifiedUser(long osuId, long discordUserId) : this()
        {
            OsuId = osuId;
            DiscordUserId = discordUserId;
        }

        public static implicit operator VerifiedUser(SkyBot.Database.Models.User user)
        {
            return new VerifiedUser(user.OsuUserId, user.DiscordUserId);
        }
    }
}
