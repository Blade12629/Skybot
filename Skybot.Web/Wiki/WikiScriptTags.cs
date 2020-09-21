using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skybot.Web.Wiki
{
    public static class WikiScriptTags
    {
        public const string TagStart = "@-@___{";
        public const string TagEnd = "}___@-@";

        public static readonly string PageStart = $"{TagStart}PAGESTART{TagEnd}";
        public static readonly string PageEnd = $"{TagStart}PAGEEND{TagEnd}";
        public static readonly string ContentReplacer = $"{TagStart}HTMLTEXT{TagEnd}";
    }
}
