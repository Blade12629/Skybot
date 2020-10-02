using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Skybot.Web.Pages.Api
{
    public struct ApiResponse
    {
        public string Code { get; }
        public string Message { get; }

        public ApiResponse(HttpStatusCode code, string message) : this()
        {
            Code = $"{(int)code} - {code}";
            Message = message;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static implicit operator string(ApiResponse response)
        {
            return response.ToString();
        }
    }
}
