using System.Linq;
using NGator.Net.Infrastructure;

namespace NGator.Net.Models.Parsers
{
    /// <summary>
    /// Vz.ru parser
    /// </summary>
    public class VzRuParser : ArticleParserAbstract
    {
        /// <summary>
        /// Parses article from given Url
        /// </summary>
        /// <param name="article">Article container. Url is stored inside </param>
        /// <returns>Success indication flag</returns>
        public override bool ParseArticle(ArticleContainer article)
        {
            var task = GetArticle(article.Header.Link, "windows-1251");
            task.Wait();
            if (task.Result == null)
                return false;

            var doc = task.Result;
            var nodeList =
                doc.DocumentNode.Descendants()
                    .Where(
                        x => x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value == "text")
                    .ToList();
            var newsBody = nodeList.FirstOrDefault();

            if (newsBody != null)
            {
                var strippedContent = newsBody.InnerHtml.RemoveDivAndScriptBlocks().StripTagsRegex();

                article.Body = new BodyContainer
                {
                    Body = strippedContent,
                    HasPicture = false
                };

                return true;
            }
            return false;
        }
    }
}