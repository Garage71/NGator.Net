using System;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using NGator.Net.Models.Parsers;

namespace NGator.Net.Models
{
    /// <summary>
    /// IParserProvider implementation
    /// </summary>
    public class ParserProvider : IParserProvider
    {
        private static readonly RiaParser RiaParser = new RiaParser();

        /// <summary>
        ///     Simple map containing instances of parsers
        /// </summary>
        private readonly Dictionary<string, ArticleParserAbstract> _parsersMap = new Dictionary
            <string, ArticleParserAbstract>
        {
            {"Lenta.ru", new LentaParser()},
            {"Regnum", new RegnumParser()},
            {"News.mail.ru", new NewsMailRuParser()},
            {"RIA World", RiaParser},
            {"RIA Economics", RiaParser},
            {"RIA Politics", RiaParser},
            {"News.rambler.ru", new NewsRamblerRuParser()},
            {"VZ.ru", new VzRuParser()}
        };

        private readonly IContentStorage _storage;

        /// <summary>
        ///     Public constructor used for dependency injection
        /// </summary>
        /// <param name="storage"></param>
        public ParserProvider(IContentStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        ///     Obtains article w/parsed body
        /// </summary>
        /// <param name="guid">Guid of article</param>
        /// <returns>Article</returns>
        public ArticleContainer GetArticleBody(Guid guid)
        {
            var task = _storage.GetArticleByGuid(guid);
            task.Wait();
            if (task.Result != null)
            {
                var article = task.Result;
                var source = article.RssSource.SiteName;
                if (article.Body != null && !article.Body.Body.IsNullOrWhiteSpace())
                    return article;
                ArticleParserAbstract parser;
                if (_parsersMap.TryGetValue(source, out parser))
                {
                    if (parser.ParseArticle(article))
                        return article;

                    //todo Implement error reporting engine
                    var error = parser.GetError();
                    return null;
                }
                return null;
            }
            return null;
        }
    }
}