using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGator.Net.Models
{
    /// <summary>
    /// Simple and fast memory IContentStorage implementation
    /// </summary>
    public class MemoryContentStorage : IContentStorage
    {
        private readonly Dictionary<Guid, ArticleContainer> _articleStorage = new Dictionary<Guid, ArticleContainer>();
        private readonly Dictionary<string, Guid> _urlToGuidMap = new Dictionary<string, Guid>();

        /// <summary>
        /// Saves the article
        /// </summary>
        /// <param name="article">Article container</param>
        /// <returns>Operation result</returns>
        public bool SaveArticle(ArticleContainer article)
        {
            _articleStorage[article.Guid] = article;
            _urlToGuidMap[article.Header.Link] = article.Guid;
            return true;
        }

        /// <summary>
        /// Obtains articles by their RSS source
        /// </summary>
        /// <param name="source">RSS source</param>
        /// <returns>List of articles</returns>
        public List<ArticleContainer> GetArticlesBySource(RssSource source)
        {
            return _articleStorage.Values.Where(art => art.Header.Source == source.SiteName).ToList();
        }

        /// <summary>
        /// Obtains article by it's Url. Asynchronous call
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Article</returns>
        public Task<ArticleContainer> GetArticleByUrl(string url)
        {
            ArticleContainer article = null;
            Guid guid;
            if (_urlToGuidMap.TryGetValue(url, out guid))
                article = _articleStorage[guid];

            var task = new Task<ArticleContainer>(() => article);
            task.Start();
            return task;
        }

        /// <summary>
        /// Obtains article by it's Guid. Asynchronous call
        /// </summary>
        /// <param name="guid">Article Guid</param>
        /// <returns>Article</returns>
        public Task<ArticleContainer> GetArticleByGuid(Guid guid)
        {
            ArticleContainer container;
            _articleStorage.TryGetValue(guid, out container);
            var task = new Task<ArticleContainer>(() => container);
            task.Start();
            return task;
        }
    }
}