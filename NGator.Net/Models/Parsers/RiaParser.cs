using System;
using System.Linq;
using System.Text;
using NGator.Net.Infrastructure;

namespace NGator.Net.Models.Parsers
{
    /// <summary>
    ///     Ria parser
    /// </summary>
    public class RiaParser : ArticleParserAbstract
    {
        /// <summary>
        ///     Parses article from given Url
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
                     x.Attributes["class"].Value.Contains("article_full_text"))).ToList();
            var newsBody = nodeList.FirstOrDefault();
            var sb = new StringBuilder();
            if (newsBody != null)
            {
                foreach (var node in newsBody.ChildNodes.Where(n => n.Name == "p"))
                    sb.Append(node.InnerText.Replace("&nbsp;", " ") + " ");

                var pictDiv =
                    newsBody.ChildNodes.FirstOrDefault(div => div.Name == "div" && div.Attributes["class"] != null &&
                                                              div.Attributes["class"].Value == "article_illustration");

                if (pictDiv != null)
                {
                    var img =
                        pictDiv.Descendants()
                            .FirstOrDefault(node => node.Name == "img" && node.Attributes["itemprop"] != null &&
                                                    node.Attributes["itemprop"].Value == "associatedMedia");
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
                }
                return true;
            }
            return false;
        }
    }
}