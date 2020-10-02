using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skybot.Web.Pages.Api
{
    public struct ApiObject<T>
    {
        public T Object { get; }

        public ApiObject(T obj)
        {
            Object = obj;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Object, Formatting.Indented);
        }

        public static implicit operator string(ApiObject<T> obj)
        {
            return obj.ToString();
        }
    }
}
