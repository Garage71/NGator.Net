using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json.Linq;
using NGator.Net.Models;

namespace NGator.Net.Controllers
{
    /// <summary>
    /// MVC 5 / Ajax news controller
    /// </summary>
    [RoutePrefix("api/sources")]
    public class SourcesController : ApiController
    {
        private readonly IContentStorage _contentStorage;
        private readonly MediaTypeHeaderValue _contentType = new MediaTypeHeaderValue("image/png");
        private readonly IParserProvider _newsParser;
        private readonly INewsProvider _newsProvider;
        private readonly IRssSourcesProvider _rssSourcesProvider;

        #region Default constructor

        /// <summary>
        /// Public constructor used for dependencies injection
        /// </summary>
        /// <param name="rssSourcesProvider">RSS Sources prodiver instance</param>
        /// <param name="newsProvider">New provider instance</param>
        /// <param name="storage">Storage provider instance</param>
        /// <param name="parser">Parser provider instance</param>
        public SourcesController(IRssSourcesProvider rssSourcesProvider, INewsProvider newsProvider,
            IContentStorage storage, IParserProvider parser)
        {
            _rssSourcesProvider = rssSourcesProvider;
            _newsProvider = newsProvider;
            _contentStorage = storage;
            _newsParser = parser;
        }

        #endregion

        #region REST/ Ajax implementation

        /// <summary>
        /// Loads article body. Asynchronous call
        /// </summary>
        /// <param name="id">Article Guid</param>
        /// <returns>IHttpActionResult w/article body</returns>
        [Route("article")]
        [HttpGet]
        public async Task<IHttpActionResult> GetArticle(Guid id)
        {
            var article = await GetArticleBody(id);
            if (article == null)
                return NotFound();

            return Ok(article.Body);
        }

        /// <summary>
        /// Loads site logo picture. Asynchronous call
        /// </summary>
        /// <param name="id">Article Guid</param>
        /// <returns>HttpResponseMessage w/logo picture</returns>
        [Route("logo")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetLogo(string id)
        {
            return await GetPicture(id, true);
        }

        /// <summary>
        /// Loads article illustration. Asynchronous call
        /// </summary>
        /// <param name="id"></param>
        /// <returns>HttpResponseMessage w/enclosure picture</returns>
        [Route("picture")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetEnclosure(string id)
        {
            return await GetPicture(id, false);
        }

        /// <summary>
        /// Main REST Get method. Loads list of RSS sources
        /// </summary>
        /// <returns>IHttpActionResult w/Rss sources list container</returns>
        [ResponseType(typeof (RssSources))]
        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            var sources = await GetSources();
            var res = Ok(sources);
            return res;
        }

        /// <summary>
        /// Main REST Post method. Loads news headers from selected RSS Sources
        /// </summary>
        /// <param name="jsonData">News body request in JSON format. Contains page number and sources list</param>
        /// <returns>IHttpActionResult w/news headers list</returns>
        [HttpPost]
        [ResponseType(typeof (RssSources))]
        public async Task<IHttpActionResult> Post(JObject jsonData)
        {
            int page;
            RssSources sources;
            bool refresh;
            try
            {
                dynamic jSon = jsonData;

                var jPage = jSon["page"];
                var jSources = jSon["sources"];
                var jRefresh = jSon["refresh"];

                page = jPage.ToObject<int>();
                refresh = jRefresh.ToObject<bool>();
                var jarray = jSources.ToObject<JArray>();
                var list = new List<RssSource>();
                foreach (var src in jarray)
                {
                    list.Add(new RssSource
                    {
                        SiteName = (string) src["sitename"]
                    });
                }
                sources = new RssSources
                {
                    Sources = list
                };
            }
            catch
            {
                return BadRequest();
            }
            var headers = await GetNews(sources, page, refresh);
            return Ok(headers);
        }

        #endregion

        #region Private methods

        private Task<RssSources> GetSources()
        {
            var task = new Task<RssSources>(() => _rssSourcesProvider.GetRssSources());
            task.Start();
            return task;
        }

        private Task<NewsHeaders> GetNews(RssSources sources, int page, bool refresh)
        {
            var task =
                new Task<NewsHeaders>(
                    () =>
                        _newsProvider.GetNews(_rssSourcesProvider.GetRssSources(sources.Sources).Sources, page, refresh));
            task.Start();
            return task;
        }

        private Task<ArticleContainer> GetArticleBody(Guid guid)
        {
            var task = new Task<ArticleContainer>(() => _newsParser.GetArticleBody(guid));
            task.Start();
            return task;
        }

        private async Task<HttpResponseMessage> GetPicture(string id, bool isLogo)
        {
            Guid guid;
            Guid.TryParse(id, out guid);

            var article = await _contentStorage.GetArticleByGuid(guid);

            var response = new HttpResponseMessage();
            if (article != null && (article.RssSource.Logo != null || article.Header.Enclosure != null))
            {
                response.StatusCode = HttpStatusCode.OK;

                var stream = isLogo ? article.RssSource.Logo : article.Header.Enclosure;

                if (stream != null)
                {
                    var ms = new MemoryStream(stream);
                    response.Content = new StreamContent(ms);
                    response.Content.Headers.ContentType = _contentType;
                }
                else
                    response.StatusCode = HttpStatusCode.NotFound;
            }
            else
                response.StatusCode = HttpStatusCode.NotFound;

            return response;
        }

        #endregion
    }
}