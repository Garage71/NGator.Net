using System;
using System.Linq;
using System.Text;
using System.Web.Razor.Generator;
using HtmlAgilityPack;
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
                    x.Name == "div" && x.Attributes["class"] != null &&
                     x.Attributes["class"].Value.Contains("article__text js-module js-mediator-article")).ToList();
            var newsBody = nodeList.FirstOrDefault();
            var sb = new StringBuilder();
            if (newsBody != null)
            {
                var indexNode = GetDescendantByAttributes(newsBody, "div", "class", "article__item_html");

                if (indexNode != null)
                {
                    foreach (var node in indexNode.ChildNodes.Where(n => n.Name == "p"))
                        sb.Append(node.InnerText.Replace("&nbsp;", " ") + " ");
                    
                    var img = GetDescendantByAttributes(newsBody, "img", "class", "photo__pic");

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
            }
            return false;
        }

        private HtmlNode GetDescendantByAttributes(HtmlNode node, string name, string attribute, string value)
        {
            if (node.Name == name && node.HasAttributes)
            {
                var attr = node.GetAttributeValue(attribute, "");
                if (!string.IsNullOrEmpty(attr) && attr.Contains(value))
                    return node;
            }
            if (node.HasChildNodes)
            {
                foreach (var childNode in node.ChildNodes)
                {
                    var result = GetDescendantByAttributes(childNode, name, attribute, value);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }        
    }
}