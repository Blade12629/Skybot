using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Skybot.Web.Authorization
{
    public class ApiKeyHandler : AuthenticationHandler<ApiKeyOptions>
    {
        public IServiceProvider ServiceProvider { get; set; }

        public ApiKeyHandler(IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IServiceProvider serviceProvider)
            : base(options, logger, encoder, clock)
        {
            ServiceProvider = serviceProvider;
        }


        /// <summary>
        /// Hashes a string and returns it's hashed base64 representation
        /// </summary>
        private static string HashKey(string key)
        {
            using (SHA512 sha = SHA512.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] hash = sha.ComputeHash(keyBytes);

                return Convert.ToBase64String(hash);
            }
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string key = Request.Headers[AuthenticationSchemes.ApiKeyScheme];

            if (string.IsNullOrEmpty(key))
            {
                if (string.IsNullOrEmpty(Request.Query[AuthenticationSchemes.ApiKeyScheme]))
                    return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));

                key = Request.Query[AuthenticationSchemes.ApiKeyScheme];
            }

            string hashedKey = HashKey(key);

            using DBContext c = new DBContext();
            APIUser user = c.APIUser.FirstOrDefault(u => u.APIKeyMD5.Equals(hashedKey, StringComparison.CurrentCulture));

            if (user == null)
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));

            Claim[] claims = new Claim[] { new Claim("token", hashedKey) };
            ClaimsIdentity identity = new ClaimsIdentity(claims, nameof(ApiKeyHandler));
            AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
