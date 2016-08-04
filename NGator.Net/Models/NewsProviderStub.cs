using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NGator.Net.Models
{
    /// <summary>
    /// Stub INewsProvider implementation
    /// </summary>
    public class NewsProviderStub : INewsProvider
    {
        /// <summary>
        /// Loads news from multiple RSS sources
        /// </summary>
        /// <param name="sources">RSS Sources list</param>
        /// <param name="page">Current page</param>
        /// <param name="refresh">Flag indicating that news should be refreshed</param>
        /// <returns>News headers</returns>
        public NewsHeaders GetNews(RssSources sources, int page, bool refresh)
        {
            var headers = new List<NewsHeader>();
            var randomizer = new Random();
            var amount = (int) Math.Ceiling(randomizer.NextDouble()*10);
            for (var i = 0; i < amount; i++)
            {
                headers.Add(new NewsHeader
                {
                    Guid = Guid.NewGuid(),
                    PublishDate = DateTime.Now.AddMinutes(-i),
                    Title =
                        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
                });
            }

            var timeToSleep = (int) (randomizer.NextDouble()*10000);
            Thread.Sleep(timeToSleep);
            return new NewsHeaders
            {
                Headers = headers
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
            throw new NotImplementedException();
        }
    }
}