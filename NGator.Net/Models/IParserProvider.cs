using System;

namespace NGator.Net.Models
{
    /// <summary>
    /// Parser provider interface
    /// </summary>
    public interface IParserProvider
    {
        /// <summary>
        /// Obtains article w/parsed body
        /// </summary>
        /// <param name="guid">Guid of article</param>
        /// <returns>Article</returns>
        ArticleContainer GetArticleBody(Guid guid);
    }
}