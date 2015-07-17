using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using NGator.Net.App_Start;
using NGator.Net.Infrastructure;
using Ninject;

namespace NGator.Net.Models
{
    /// <summary>
    /// IRssProvider implementation
    /// </summary>
    public class RssProvider : IRssProvider, IErrorDescriptor
    {
        private readonly IContentStorage _contentStorage;
        private readonly HttpClient _httpClient;
        private ErrorDescription _errorDescription;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RssProvider()
        {
            _httpClient = new HttpClient();
            var kernel = NinjectWebCommon.Kernel;
            _contentStorage = kernel.Get<IContentStorage>(); // direct ise of dependency injection
        }

        /// <summary>
        /// Obtains error description
        /// </summary>
        /// <returns>Error description</returns>
        public ErrorDescription GetError()
        {
            return _errorDescription;
        }

        /// <summary>
        /// Obtains article headers from RSS source
        /// </summary>
        /// <param name="source">Rss source</param>
        /// <param name="refresh">Flad indicating news should be refreshed</param>
        /// <returns>News headers</returns>
        public List<NewsHeader> GetArticlesHeaders(RssSource source, bool refresh)
        {
            var articleHeaders = new List<NewsHeader>();
            var rssFeed = new XmlDocument();

            #region new data loading

            if (refresh)
            {
                try
                {
                    rssFeed.Load(source.Url); // todo refactor this with HttpClient

                    // Load logo if present

                    if (source.Logo == null)
                    {
                        var logoNodes = rssFeed.SelectNodes("rss/channel/image");
                        if (logoNodes != null)
                        {
                            foreach (var logoUrl in 
                                from XmlNode inner
                                    in logoNodes
                                select inner.SelectSingleNode("url")
                                into urlNode
                                where urlNode != null
                                select urlNode.InnerText)
                            {
                                GetContent(logoUrl).ContinueWith(response =>
                                {
                                    var content = response.Result.Content as StreamContent;
                                    if (content != null)
                                        content.ReadAsByteArrayAsync()
                                            .ContinueWith(bytes => source.Logo = bytes.Result);
                                }).Wait();
                                break;
                            }
                        }
                    }

                    var rssNodes = rssFeed.SelectNodes("rss/channel/item");
                    if (rssNodes != null)
                    {
                        foreach (XmlNode rssNode in rssNodes)
                        {
                            var rssSubNode = rssNode.SelectSingleNode("link");
                            var link = rssSubNode != null ? rssSubNode.InnerText : "";

                            var task = _contentStorage.GetArticleByUrl(link);
                            task.Wait();

                            if (task.Result != null)
                            {
                                articleHeaders.Add(task.Result.Header);
                                continue;
                            }

                            rssSubNode = rssNode.SelectSingleNode("title");
                            var title = rssSubNode != null ? rssSubNode.InnerText.Replace("&mdash;", "-") : "";

                            rssSubNode = rssNode.SelectSingleNode("description");
                            var description = rssSubNode != null ? rssSubNode.InnerText.StripTagsRegex() : "";
                            if (description.Length > 150)
                                description = description.Substring(0, 147) + "..."; // trim too long description for pop-over

                            rssSubNode = rssNode.SelectSingleNode("pubDate");
                            var date = rssSubNode != null ? rssSubNode.InnerText : "";

                            rssSubNode = rssNode.SelectSingleNode("enclosure");
                            var enclosure = rssSubNode != null && rssSubNode.Attributes != null
                                ? rssSubNode.Attributes["url"].Value
                                : "";
                            byte[] enclosured = null;
                            if (enclosure != "")
                            {
                                GetContent(enclosure).ContinueWith(response =>
                                {
                                    var content = response.Result.Content as StreamContent;
                                    if (content != null)
                                        content.ReadAsByteArrayAsync()
                                            .ContinueWith(bytes => enclosured = bytes.Result);
                                }).Wait();
                            }
                            var newsHeader = new NewsHeader
                            {
                                Description = description,
                                Link = link,
                                Title = title,
                                Guid = Guid.NewGuid(),
                                PublishDate = DateTime.Parse(date),
                                Enclosure = enclosured,
                                Source = source.SiteName,
                                HasLogo = source.Logo != null,
                                HasEnclosure = enclosured != null
                            };
                            articleHeaders.Add(newsHeader);
                            _contentStorage.SaveArticle(new ArticleContainer
                            {
                                Guid = newsHeader.Guid,
                                RssSource = source,
                                Header = newsHeader
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorDescription = new ErrorDescription
                    {
                        Level = ApplicationLevel.RssParser,
                        Description = ex.Message
                    };
                }
            }

            #endregion
            #region obtaining loaded data
            else
                articleHeaders = _contentStorage.GetArticlesBySource(source).Select(art => art.Header).ToList();
            #endregion

            return articleHeaders;
        }

        private async Task<HttpResponseMessage> GetContent(string url)
        {
            try
            {
                return await _httpClient.GetAsync(url);
            }
            catch (Exception ex)
            {
                _errorDescription = new ErrorDescription
                {
                    Level = ApplicationLevel.RssParser,
                    Description = ex.Message
                };
            }
            return null;
        }
    }
}