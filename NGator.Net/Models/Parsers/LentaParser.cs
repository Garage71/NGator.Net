using System.Linq;
using System.Text;

namespace NGator.Net.Models.Parsers
{
    /// <summary>
    /// Lenta.ru parser
    /// </summary>
    public class LentaParser : ArticleParserAbstract
    {
        /// <summary>
        /// Parses article from given Url
        /// </summary>
        /// <param name="article">Article container. Url is stored inside </param>
        /// <returns>Success indication flag</returns>
        public override bool ParseArticle(ArticleContainer article)
        {
            var task = GetArticle(article.Header.Link);
            task.Wait();
            if (task.Result == null)
                return false;

            var doc = task.Result;
            var nodeList = doc.DocumentNode.Descendants().Where
                (x =>
                    (x.Name == "div" && x.Attributes["itemprop"] != null &&
                     x.Attributes["itemprop"].Value.Contains("articleBody"))).ToList();

            var sb = new StringBuilder();

            var newsBody = nodeList.FirstOrDefault();
            if (newsBody != null)
            {
                foreach (var node in newsBody.ChildNodes.Where(n => n.Name == "p"))
                {
                    sb.Append(node.InnerText + " ");
                }
                article.Body = new BodyContainer
                {
                    Body = sb.ToString(),
                    HasPicture = false
                };
                return true;
            }
            return false;
        }
    }
}