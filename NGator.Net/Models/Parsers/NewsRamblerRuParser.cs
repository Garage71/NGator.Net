using System;
using System.Linq;
using System.Text;
using NGator.Net.Infrastructure;

namespace NGator.Net.Models.Parsers
{
    /// <summary>
    /// News.rambler.ru parser
    /// </summary>
    public class NewsRamblerRuParser : ArticleParserAbstract
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
            var nodeList = doc.DocumentNode.Descendants().Where(x => x.Name == "div" && x.Attributes["class"] != null &&
                           x.Attributes["class"].Value.Contains("article__column")).ToList();
            var newsBody = nodeList.FirstOrDefault();
            var sb = new StringBuilder();
            if (newsBody != null)
            {
                foreach (var node in newsBody.ChildNodes.Where(n => n.Name == "div" && n.Attributes["class"] != null &&
                           n.Attributes["class"].Value.Contains("article__paragraph")))
                    sb.Append(node.InnerText + " ");

                var imgList = newsBody.Descendants().Where
                    (x => (x.Name == "img" && x.Attributes["class"] != null &&
                           x.Attributes["class"].Value.Contains("article__image")))
                    .ToList();
                var img = imgList.FirstOrDefault();

                var hasPicture = false;

                if (img != null)
                {
                    try
                    {
                        var url = img.Attributes["src"].Value;

                        var binaryTask = GetBinaryContent(url);
                        binaryTask.Wait();
                        if (binaryTask.Result != null)
                        {
                            var pict = binaryTask.Result;
                            article.Header.Enclosure = pict;
                            hasPicture = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Error = new ErrorDescription
                        {
                            Level = ApplicationLevel.NewsParser,
                            Description = ex.Message
                        };
                    }
                }

                article.Body = new BodyContainer
                {
                    Body = sb.ToString(),
                    HasPicture = hasPicture
                };

                return true;
            }
            return false;
        }
    }
}