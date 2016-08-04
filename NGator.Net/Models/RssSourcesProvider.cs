using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;

namespace NGator.Net.Models
{
    /// <summary>
    ///     Rss sources provider implementation.
    ///     In this project URLs from Web.config application setting section are used with keys with rss: prefix.
    /// </summary>
    public class RssSourcesProvider : IRssSourcesProvider
    {
        /// <summary>
        ///  Obtains list of RSS sources from Web.config
        /// </summary>
        /// <returns>List of Rss source URLs</returns>
        public RssSources GetRssSources(RssSources sources)
        {
            var appsettings = WebConfigurationManager.AppSettings;
            var srcs = new RssSources
            {
                Sources = (from object key in appsettings.Keys
                    select key.ToString()
                    into strKey
                    let splitted = strKey.Split(':')
                    where splitted[0] == "rss"
                    let url = WebConfigurationManager.AppSettings[strKey]
                    select new RssSource
                    {
                        SiteName = splitted[1],
                        Url = url
                    }).ToList()
            };
            if (sources != null)
                srcs.Sources = srcs.Sources.Where(src => sources.Sources.Any(site => site.SiteName == src.SiteName)).ToList();
            
            return srcs;
        }
    }
}