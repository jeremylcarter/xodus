using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Xodus
{
    public class TVMaze
    {
        public Dictionary<string, string> GetChannels()
        {
            var newList = new Dictionary<string, string>();
            newList.Add("A&E", "/networks/29/ae");
            newList.Add("ABC", "/networks/3/abc");
            newList.Add("AMC", "/networks/20/amc");
            newList.Add("AT-X", "/networks/167/at-x");
            newList.Add("Adult Swim", "/networks/10/adult-swim");
            newList.Add("Amazon", "/webchannels/3/amazon");
            newList.Add("Animal Planet", "/networks/92/animal-planet");
            newList.Add("Audience", "/networks/31/audience-network");
            newList.Add("BBC America", "/networks/15/bbc-america");
            newList.Add("BBC Four", "/networks/51/bbc-four");
            newList.Add("BBC One", "/networks/12/bbc-one");
            newList.Add("BBC Three", "/webchannels/71/bbc-three");
            newList.Add("BBC Two", "/networks/37/bbc-two");
            newList.Add("BET", "/networks/56/bet");
            newList.Add("Bravo", "/networks/52/bravo");
            newList.Add("CBC", "/networks/36/cbc");
            newList.Add("CBS", "/networks/2/cbs");
            newList.Add("CTV", "/networks/48/ctv");
            newList.Add("CW", "/networks/5/the-cw");
            newList.Add("CW Seed", "/webchannels/13/cw-seed");
            newList.Add("Cartoon Network", "/networks/11/cartoon-network");
            newList.Add("Channel 4", "/networks/45/channel-4");
            newList.Add("Channel 5", "/networks/135/channel-5");
            newList.Add("Cinemax", "/networks/19/cinemax");
            newList.Add("Comedy Central", "/networks/23/comedy-central");
            newList.Add("Crackle", "/webchannels/4/crackle");
            newList.Add("Discovery Channel", "/networks/66/discovery-channel");
            newList.Add("Discovery ID", "/networks/89/investigation-discovery");
            newList.Add("Disney Channel", "/networks/78/disney-channel");
            newList.Add("Disney XD", "/networks/25/disney-xd");
            newList.Add("E! Entertainment", "/networks/43/e");
            newList.Add("E4", "/networks/41/e4");
            newList.Add("FOX", "/networks/4/fox");
            newList.Add("FX", "/networks/13/fx");
            newList.Add("Freeform", "/networks/26/freeform");
            newList.Add("HBO", "/networks/8/hbo");
            newList.Add("HGTV", "/networks/192/hgtv");
            newList.Add("Hallmark", "/networks/50/hallmark-channel");
            newList.Add("History Channel", "/networks/53/history");
            newList.Add("ITV", "/networks/35/itv");
            newList.Add("Lifetime", "/networks/18/lifetime");
            newList.Add("MTV", "/networks/22/mtv");
            newList.Add("NBC", "/networks/1/nbc");
            newList.Add("National Geographic", "/networks/42/national-geographic-channel");
            newList.Add("Netflix", "/webchannels/1/netflix");
            newList.Add("Nickelodeon", "/networks/27/nickelodeon");
            newList.Add("PBS", "/networks/85/pbs");
            newList.Add("Showtime", "/networks/9/showtime");
            newList.Add("Sky1", "/networks/63/sky-1");
            newList.Add("Starz", "/networks/17/starz");
            newList.Add("Sundance", "/networks/33/sundance-tv");
            newList.Add("Syfy", "/networks/16/syfy");
            newList.Add("TBS", "/networks/32/tbs");
            newList.Add("TLC", "/networks/80/tlc");
            newList.Add("TNT", "/networks/14/tnt");
            newList.Add("TV Land", "/networks/57/tvland");
            newList.Add("Travel Channel", "/networks/82/travel-channel");
            newList.Add("TruTV", "/networks/84/trutv");
            newList.Add("USA", "/networks/30/usa-network");
            newList.Add("VH1", "/networks/55/vh1");
            newList.Add("WGN", "/networks/28/wgn-america");
            return newList;
        }

        public async Task<List<string>> List(string uri)
        {
            var shows = new List<string>();
            try
            {
                var httpClient = Utilities.GetHttpClient();
                var url = $"http://www.tvmaze.com{uri}";
                var result = await httpClient.GetStringAsync(url);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result);
                var items = htmlDocument.DocumentNode.Descendants("section").FirstOrDefault(
                    x => x.Attributes.Contains("id")
                         && x.Attributes["id"].Value == "this-seasons-shows");


                var lists = items.Descendants("li");

                foreach (var z in lists)
                {
                    var a = z.Descendants("a").Where(x => x.Attributes.Contains("href"));
                    foreach (var n in a)
                        shows.Add(n.InnerText);
                }
            }
            catch (Exception)
            {
            }
            return shows;
        }
    }
}