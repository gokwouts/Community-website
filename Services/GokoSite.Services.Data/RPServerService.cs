namespace GokoSite.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    using GokoSite.Web.ViewModels.Players;

    public class RPServerService : IRPServerService
    {
        private const string ip = "93.123.18.103";
        private const string port = "30120";

        private string Host(string endpoint)
        {
            return "http://" + $"{ip}:{port}" + $"/{endpoint}.json";
        }

        public HomePageViewModel GetPlayers()
        {
            List<string> names = new List<string>();
            List<int> pings = new List<int>();

            string url = this.Host("players");
            var req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException("error code: " + res.StatusCode);
                }

                using (Stream resStream = res.GetResponseStream())
                {
                    if (resStream != null)
                    {
                        using (StreamReader reader = new StreamReader(resStream))
                        {
                            string[] result = reader.ReadToEnd().Split(new char[] { '[', ']', '\\', '\"', '{', '}', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < result.Length; i++)
                            {
                                if (result[i] == "name")
                                {
                                    names.Add(result[i + 1]);
                                }
                                if (result[i] == "ping")
                                {
                                    pings.Add(int.Parse(result[i + 1]));
                                }
                            }

                            reader.Dispose();
                        }
                    }

                    resStream.Dispose();
                }

                res.Dispose();
            }

            return new HomePageViewModel { PlayerNames = names, PlayerPings = pings, PlayersCount = names.Count };
        }
    }
}
