using System;
using System.Linq;
using System.Text;
using NGator.Net.Infrastructure;

namespace NGator.Net.Models.Parsers
{
    /// <summary>
    /// News.mail.ru parser
    /// </summary>
    public class NewsMailRuParser : ArticleParserAbstract
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
                    (x.Name == "div" && x.Attributes["class"] != null &&
                     x.Attributes["class"].Value.Contains("js-newstext"))).ToList();
            var newsBody = nodeList.FirstOrDefault();
            var sb = new StringBuilder();
            if (newsBody != null)
            {
                var indexNode = newsBody.ChildNodes.FirstOrDefault(n => n.Name == "index");
                if (indexNode != null)
                {
                    foreach (var node in indexNode.ChildNodes.Where(n => n.Name == "p"))
                        sb.Append(node.InnerText.Replace("&nbsp;", " ") + " ");

                    var imgList = doc.DocumentNode.Descendants().Where
                        (x =>
                            (x.Name == "img" && x.Attributes["class"] != null &&
                             x.Attributes["class"].Value.Contains("c-photoblock__image__inner__self js-gallery__preview")))
                        .ToList();
                    var img = imgList.FirstOrDefault();

                    var hasPicture = false;

                    if (img != null)
                    {
                        try
                        {
                            var url = img.Attributes["src"].Value;

                            var binaryTask = GetBinaryContent("http:" + url);
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
            }
            return false;
        }
    }
}