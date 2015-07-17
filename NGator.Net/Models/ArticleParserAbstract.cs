using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NGator.Net.Infrastructure;

namespace NGator.Net.Models
{
    /// <summary>
    /// Abstract base parser class
    /// </summary>
    public abstract class ArticleParserAbstract : IErrorDescriptor
    {
        private readonly HttpClient _httpClient = new HttpClient();
        protected ErrorDescription Error;

        /// <summary>
        /// Obtains error description
        /// </summary>
        /// <returns>Error description</returns>
        public ErrorDescription GetError()
        {
            return Error;
        }

        /// <summary>
        /// Parses article from given Url
        /// </summary>
        /// <param name="article">Article container. Url is stored inside </param>
        /// <returns>Success indication flag</returns>
        public abstract bool ParseArticle(ArticleContainer article);

        #region Protected members

        protected async Task<HtmlDocument> GetArticle(string url, string encoding = "")
        {
            HtmlDocument document = null;
            try
            {
                var response = await _httpClient.GetByteArrayAsync(url);
                if (encoding == "")
                    encoding = "utf-8";
                var source = Encoding.GetEncoding(encoding).GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                document = new HtmlDocument();
                document.LoadHtml(source);
            }
            catch (Exception ex)
            {
                Error = new ErrorDescription
                {
                    Level = ApplicationLevel.NewsParser,
                    Description = ex.Message
                };
            }
            return document;
        }

        protected async Task<byte[]> GetBinaryContent(string url)
        {
            return await _httpClient.GetByteArrayAsync(url);
        }

        #endregion
    }
}