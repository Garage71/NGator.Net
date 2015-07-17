using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NGator.Net.Models
{
    /// <summary>
    /// News provider interface
    /// </summary>
    public interface INewsProvider
    {
        /// <summary>
        /// Loads news from multiple RSS sources
        /// </summary>
        /// <param name="sources">RSS Sources list</param>
        /// <param name="page">Current page</param>
        /// <param name="refresh">Flag indicating that news should be refreshed</param>
        /// <returns>News headers</returns>
        NewsHeaders GetNews(List<RssSource> sources, int page, bool refresh);

        /// <summary>
        /// Loads news from single RSS source. Asynchronous call
        /// </summary>
        /// <param name="source">Rss source</param>
        /// <param name="refresh">Flag indicating that news should be refreshed</param>
        /// <returns>News headers</returns>
        Task<NewsHeaders> GetNewsFromSingleSource(RssSource source, bool refresh);
    }

    /// <summary>
    /// News header entity
    /// </summary>
    public class NewsHeader
    {
        /// <summary>
        /// Article title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Article Url
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Short description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Article publish date
        /// </summary>
        public DateTime PublishDate { get; set; }
        /// <summary>
        /// Guid of the article
        /// </summary>
        public Guid Guid { get; set; }
        /// <summary>
        /// Illustration picture
        /// </summary>
        [JsonIgnore]
        public byte[] Enclosure { get; set; }
        /// <summary>
        /// Site source
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Source has logo picture
        /// </summary>
        public bool HasLogo { get; set; }
        /// <summary>
        /// Article has illustration enclosure
        /// </summary>
        public bool HasEnclosure { get; set; }
    }

    /// <summary>
    /// Headers container entity
    /// </summary>
    public class NewsHeaders
    {
        /// <summary>
        /// Total available articles count
        /// </summary>
        public int TotalArticlesCount { get; set; }
        /// <summary>
        /// News headers
        /// </summary>
        public List<NewsHeader> Headers { get; set; }
    }
}