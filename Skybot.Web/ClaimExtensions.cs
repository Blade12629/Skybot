using Skybot.Web;
using SkyBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

    public static bool AllowGlobalStats(this Claim c)
    {
        return bool.Parse(c.Properties[ClaimProperties.AllowGlobalStats]);
    }

    public static long GetDiscordGuildId(this Claim c)
    {
        return long.Parse(c.Properties[ClaimProperties.DiscordGuildId]);
    }

    public static List<long> GetDiscordGuildIds(this Claim c)
    {
        int totalServers = int.Parse(c.Properties[ClaimProperties.TotalServers]);
        List<long> discordServers = new List<long>()
        {
            long.Parse(c.Properties[ClaimProperties.DiscordGuildId])
        };

        for (int i = 1; i < totalServers; i++)
            discordServers.Add(long.Parse(c.Properties[$"{ClaimProperties.DiscordGuildId}{i}"]));

        return discordServers;
    }
}