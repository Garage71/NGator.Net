using System.Collections.Generic;

namespace NGator.Net.Models
{
    /// <summary>
    /// Rss provider interface
    /// </summary>
    public interface IRssProvider
    {
        /// <summary>
        /// Obtains article headers from RSS source
        /// </summary>
        /// <param name="source">Rss source</param>
        /// <param name="refresh">Flad indicating news should be refreshed</param>
        /// <returns>News headers</returns>
        List<NewsHeader> GetArticlesHeaders(RssSource source, bool refresh);
    }
}