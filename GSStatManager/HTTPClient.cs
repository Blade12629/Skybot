using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Grapevine.Client;
using Grapevine.Interfaces.Server;
using SkyBot.API.Data.GlobalStatistics;
using SkyBot.API.Network;
using SkyBot.Database.Models.GlobalStatistics;

namespace GSStatManager
{
    public class HTTPClient : RestClient
    {
        public string ApiKey
        {
            get => _apikey;
            set => _apikey = value;
        }

        private string _apikey;
        private readonly TimeSpan _timeOut = TimeSpan.FromMinutes(5);

        public HTTPClient(string host, int port, string apikey) : base()
        {
            _apikey = apikey;
            Host = host;
            Port = port;
            RestRequest.GlobalTimeout = (int)_timeOut.TotalMilliseconds;
        }

        public void SubmitTourneyStats(GlobalStatsTournament stats)
        {
            RestRequest restRequest = new RestRequest("/globalstats/submit");
            restRequest.HttpMethod = Grapevine.Shared.HttpMethod.POST;
            restRequest.ContentType = Grapevine.Shared.ContentType.BIN;
            restRequest.ContentLength = 100000000000000000;
            restRequest.QueryString["apikey"] = _apikey;
            restRequest.Timeout = (int)_timeOut.TotalMilliseconds;

            UriBuilder builder = new UriBuilder()
            {
                Host = Host,
                Port = Port,
            };

            HttpWebRequest request = restRequest.Advanced.ToHttpWebRequest(builder, Cookies);
            Stream reqStream = request.GetRequestStream();
            BinaryAPIWriter writer = new BinaryAPIWriter(reqStream);
            
            stats.Serialize(writer);
            reqStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Console.WriteLine($"Received answer, Status Code: {response.StatusCode}, description: {response.StatusDescription ?? "none"}");
        }

        public GlobalStatsProfile GetProfile(long osuId)
        {
            RestRequest request = new RestRequest("/globalstats/getprofile");
            request.HttpMethod = Grapevine.Shared.HttpMethod.GET;
            request.QueryString["apikey"] = _apikey;
            request.QueryString["id"] = $"{osuId}";

            RestResponse response = (RestResponse)Execute(request);

            if (response.StatusCode != Grapevine.Shared.HttpStatusCode.Ok)
            {
                Console.WriteLine(response.StatusDescription);
                return null;
            }

            BinaryAPIReader reader = new BinaryAPIReader(response.Advanced.GetResponseStream());
            GlobalStatsProfile profile = new GlobalStatsProfile();
            profile.Deserialize(reader);

            return profile;
        }
    }
}
