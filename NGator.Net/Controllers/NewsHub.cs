using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using NGator.Net.Models;

namespace NGator.Net.Controllers
{
    /// <summary>
    /// SignarlR Hub for loading list of news headers
    /// </summary>
    public class NewsHub : Hub
    {
        private const int NewsPerPage = 10;
        private readonly INewsProvider _newsProvider;
        private readonly IRssSourcesProvider _rssSourcesProvider;

        /// <summary>
        /// Public constructor used for dependencies injection
        /// </summary>
        /// <param name="newsProvider">News provider instance</param>
        /// <param name="rssSourcesProvider">Rss sources provider instance</param>
        public NewsHub(INewsProvider newsProvider, IRssSourcesProvider rssSourcesProvider)
        {
            _newsProvider = newsProvider;
            _rssSourcesProvider = rssSourcesProvider;
        }

        /// <summary>
        /// Asynchronious SignalR RPC call. Loads list of news headers and reports loading progress. Called from client
        /// </summary>
        /// <param name="jsonData">Request data in JSON format. Parsed inside</param>
        /// <param name="progress">Progress repirting callback</param>
        /// <returns>Container with list of news headers</returns>
        public async Task<NewsHeaders> LoadNews(JObject jsonData, IProgress<int> progress)
        {
            int page;
            bool refresh;
            RssSources rssSources;
            try
            {
                dynamic jSon = jsonData;

                var jPage = jSon["page"];
                var jSources = jSon["sources"];
                var jRefresh = jSon["refresh"];

                page = jPage.ToObject<int>();
                refresh = jRefresh.ToObject<bool>();
                var jarray = jSources.ToObject<JArray>();
                var list = new List<RssSource>();
                foreach (var src in jarray)
                {
                    list.Add(new RssSource
                    {
                        SiteName = (string) src["sitename"]
                    });
                }
                rssSources = new RssSources
                {
                    Sources = list
                };
            }
            catch
            {
                progress.Report(100);
                return new NewsHeaders();
            }

            var sources = _rssSourcesProvider.GetRssSources(rssSources).Sources;
            var totalSourcesCount = sources.Count;
            var progressIncrement = 100/totalSourcesCount;
            var totalProgress = 0;
            var totalHeaders = new NewsHeaders
            {
                TotalArticlesCount = 0,
                Headers = new List<NewsHeader>()
            };

            foreach (var src in sources)
            {
                var headers = await _newsProvider.GetNewsFromSingleSource(src, refresh);
                totalHeaders.Headers.AddRange(headers.Headers);
                totalHeaders.TotalArticlesCount += headers.TotalArticlesCount;
                totalProgress += progressIncrement;
                progress.Report(totalProgress);
            }

            totalHeaders.Headers =
                totalHeaders.Headers.OrderByDescending(article => article.PublishDate)
                    .Where((article, i) => (i >= NewsPerPage*(page - 1)) && (i < NewsPerPage*page))
                    .ToList();
            progress.Report(100);
            return totalHeaders;
        }
    }
}