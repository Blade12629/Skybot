using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SkyBot.Osu.Web
{
    public class BadgeGrabber : IDisposable
    {
        public bool IsDisposed { get; private set; }

        private WebClient _web;

        public BadgeGrabber()
        {
            _web = new WebClient();
        }

        ~BadgeGrabber()
        {
            Dispose(false);
        }

        public int Count(long osuId)
        {
            const string userUrlPattern = "https://osu.ppy.sh/users/{0}/osu";

            string url = string.Format(userUrlPattern, osuId.ToString());
            string page = DownloadPage(url);
            page = RemovePageTop(page);
            page = RemovePageBottom(page);

            BadgeInfo[] bi;
            try
            {
                bi = Newtonsoft.Json.JsonConvert.DeserializeObject<BadgeInfo[]>(page);
            }
            catch (Exception)
            {
                Logger.Log($"No badges found for id {osuId}");
                return 0;
            }

            int counter = 0;

            for (int i = 0; i < bi.Length; i++)
            {
                StringComparison comp = StringComparison.CurrentCultureIgnoreCase;
                if (bi[i].Description.Contains("elite mapper", comp) ||
                    bi[i].Description.Contains("contribution", comp) ||
                    bi[i].Description.Contains("osu!idol", comp) ||
                    bi[i].Description.Contains("Community Favourite", comp) ||
                    bi[i].Description.Contains("spotlight", comp))
                    continue;

                counter++;
            }

            Logger.Log($"Found {counter} badges for {osuId}");

            return counter;
        }

        private string ParseBadge(string page, out string badgeString)
        {
            const string idTitle = "data-orig-title=\"";
            string result = "";

            int index = page.IndexOf(idTitle);
            if (index == -1)
            {
                badgeString = null;
                return page;
            }

            page = page.Remove(0, index + idTitle.Length + 1);
            index = page.IndexOf('"');

            result = page.Substring(0, index);

            index = page.IndexOf("</div>");
            page = page.Remove(0, index + "</div>".Length + 1);

            badgeString = result;
            return page;
        }

        private string RemovePageTop(string page)
        {
            const string identifier = ",\"badges\":[{";

            int index = page.IndexOf(identifier);

            if (index < 0)
                return page;

            string result = page.Remove(0, index + identifier.Length - 2).TrimStart('\n');

            return result.TrimStart('>', '"', '\n', ')', ' ', ']', '}');
        }

        private string RemovePageBottom(string page)
        {
            const string identifier = "\"}],\"";

            int index = page.IndexOf(identifier);

            if (index < 0)
                return page;

            string result = page.Remove(index + 3, page.Length - index - 3).TrimEnd('\n');

            return result.TrimEnd('<', '"', '\n', '(', ' ', '[', '{');
        }

        private string DownloadPage(string url)
        {
            return _web.DownloadString(url);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                _web?.Dispose();
            }

            IsDisposed = true;
        }
    }
}

