using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NGator.Net.Models
{
    /// <summary>
    /// Persistent articles storage interface
    /// </summary>
    public interface IContentStorage
    {
        /// <summary>
        /// Saves the article
        /// </summary>
        /// <param name="article">Article container</param>
        /// <returns>Operation result</returns>
        bool SaveArticle(ArticleContainer article);
        
        /// <summary>
        /// Obtains article by it's Guid. Asynchronous call
        /// </summary>
        /// <param name="guid">Article Guid</param>
        /// <returns>Article</returns>
        Task<ArticleContainer> GetArticleByGuid(Guid guid);
        
        /// <summary>
        /// Obtains articles by their RSS source
        /// </summary>
        /// <param name="source">RSS source</param>
        /// <returns>List of articles</returns>
        List<ArticleContainer> GetArticlesBySource(RssSource source);
        
        /// <summary>
        /// Obtains article by it's Url. Asynchronous call
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Article</returns>
        Task<ArticleContainer> GetArticleByUrl(string url);
    }

    /// <summary>
    /// Article container entity
    /// </summary>
    public class ArticleContainer
    {
        /// <summary>
        /// Article Guid
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Article RSS Source
        /// </summary>
        public RssSource RssSource { get; set; }

        /// <summary>
        /// Article header
        /// </summary>
        public NewsHeader Header { get; set; }

        /// <summary>
        /// Article body container
        /// </summary>
        public BodyContainer Body { get; set; }
    }

    /// <summary>
    /// Body container entity
    /// </summary>
    public class BodyContainer
    {
        /// <summary>
        /// Article body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Boolean flag indicating article has illustration picture
        /// </summary>
        public bool HasPicture { get; set; }
    }
}