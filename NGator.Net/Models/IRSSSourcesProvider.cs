using System.Collections.Generic;
using Newtonsoft.Json;

namespace NGator.Net.Models
{
    /// <summary>
    /// Interface prodiving RSS news sources
    /// </summary>
    public interface IRssSourcesProvider
    {
        /// <summary>
        ///     Gets RSS sources list
        /// </summary>
        /// <param name="sources">Filter paremeter</param>
        /// <returns>Rss sources</returns>
        RssSources GetRssSources(RssSources sources);
    }

    /// <summary>
    ///     Rss source container class
    /// </summary>
    public class RssSource
    {
        /// <summary>
        ///     Site name
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        ///     Rss url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     Site logo
        /// </summary>
        [JsonIgnore]
        public byte[] Logo { get; set; }
    }

    /// <summary>
    ///     Rss sources container
    /// </summary>
    public class RssSources
    {
        /// <summary>
        ///     Rss sources list
        /// </summary>
        public List<RssSource> Sources { get; set; }
    }
}