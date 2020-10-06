using SkyBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Skybot.Web
{
    public static class ClaimExtensions
    {
        public static Claim Claim(this ClaimsPrincipal principal)
        {
            return principal.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier));
        }

        public static string Name(this ClaimsPrincipal principal)
        {
            return principal.Claim().Value;
        }

        public static AccessLevel GetAccess(this Claim c)
        {
            return Enum.Parse<AccessLevel>(c.Properties[ClaimProperties.AccessLevel]);
        }

        public static long GetDiscordUserId(this Claim c)
        {
            return long.Parse(c.Properties[ClaimProperties.DiscordUserId]);
        }

        public static long GetDiscordGuildId(this Claim c)
        {
            return long.Parse(c.Properties[ClaimProperties.DiscordGuildId]);
        }
    }
}
