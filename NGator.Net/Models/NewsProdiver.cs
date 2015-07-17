using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGator.Net.Models
{
    /// <summary>
    /// INewsProvider implementation
    /// </summary>
    public class NewsProvider : INewsProvider
    {
        private const int NewsPerPage = 10;
        private readonly RssProvider _rssProvider = new RssProvider();

        /// <summary>
        /// Loads news from multiple RSS sources
        /// </summary>
        /// <param name="sources">RSS Sources list</param>
        /// <param name="page">Current page</param>
        /// <param name="refresh">Flag indicating that news should be refreshed</param>
        /// <returns>News headers</returns>
        public NewsHeaders GetNews(List<RssSource> sources, int page, bool refresh)
        {
            var articleHeaders = new List<NewsHeader>();
            foreach (var headers in sources.Select(src => _rssProvider.GetArticlesHeaders(src, refresh)))
                articleHeaders.AddRange(headers);

            var totalCount = articleHeaders.Count;
            articleHeaders =
                articleHeaders.OrderByDescending(article => article.PublishDate)
                    .Where((article, i) => (i >= NewsPerPage*(page - 1)) && (i < NewsPerPage*page))
                    .ToList();

            return new NewsHeaders
            {
                TotalArticlesCount = totalCount,
                Headers = articleHeaders
            };
        }

        /// <summary>
        /// Loads news from single RSS source. Asynchronous call
        /// </summary>
        /// <param name="source">Rss source</param>
        /// <param name="refresh">Flag indicating that news should be refreshed</param>
        /// <returns>News headers</returns>
        public Task<NewsHeaders> GetNewsFromSingleSource(RssSource source, bool refresh)
        {
            var task = new Task<NewsHeaders>(() =>
            {
                var headers = _rssProvider.GetArticlesHeaders(source, refresh);
                return new NewsHeaders
                {
                    Headers = headers,
                    TotalArticlesCount = headers.Count
                };
            });
            task.Start();
            return task;
        }
    }
}