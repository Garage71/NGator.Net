using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using NGator.Net.Controllers;
using NGator.Net.Models;

namespace NGator.Net.Tests.Controllers
{
    [TestClass]
    public class SourcesControllerTest
    {
        private readonly RssSource _rssSource = new RssSource
        {
            SiteName = "Lenta.ru",
            Url = "http://lenta.ru/rss"
        };

        private ArticleContainer _article1;
        private ArticleContainer _article2;
        private SourcesController _controller;
        private byte[] _enclosure;
        private NewsHeader _header1;
        private NewsHeader _header2;
        private byte[] _logo;

        [TestInitialize]
        public void Init()
        {
            //Arrange

            var rssSourcesMock = new Mock<IRssSourcesProvider>();

            rssSourcesMock.Setup(sourcesProvider => sourcesProvider.GetRssSources(It.IsAny<RssSources>()))
                .Returns(() => new RssSources
                {
                    Sources = new List<RssSource> {_rssSource}
                });

            _logo = File.ReadAllBytes("..\\..\\TestData\\logo.png");
            _enclosure = File.ReadAllBytes("..\\..\\TestData\\enclosure.jpg");
            _rssSource.Logo = _logo;

            _header1 = new NewsHeader
            {
                Guid = new Guid("10D93F92-B3E3-41F7-9E72-03A88883F6AD"),
                Description = "07/01 Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor...",
                HasEnclosure = true,
                HasLogo = true,
                Link = "http://lenta.ru/loren_ipsum_1",
                PublishDate = new DateTime(2015, 07, 01),
                Title = "Lorem ipsum 1",
                Source = "Lenta.ru",
                Enclosure = _enclosure
            };

            _header2 = new NewsHeader
            {
                Guid = new Guid("51B06028-F579-40EE-860B-A74A498A9987"),
                Description = "07/02 Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor...",
                HasEnclosure = false,
                HasLogo = true,
                Link = "http://lenta.ru/loren_ipsum_2",
                PublishDate = new DateTime(2015, 07, 02),
                Title = "Lorem ipsum 2",
                Source = "Lenta.ru"
            };

            var newsProviderMock = new Mock<INewsProvider>();
            newsProviderMock.Setup(newsProvider => newsProvider.GetNews(It.IsAny<RssSources>(), 1, true)).Returns(
                new NewsHeaders
                {
                    TotalArticlesCount = 2,
                    Headers = new List<NewsHeader> {_header1, _header2}
                });

            _article1 = new ArticleContainer
            {
                Guid = _header1.Guid,
                Header = _header1,
                RssSource = _rssSource,
                Body = new BodyContainer
                {
                    HasPicture = true,
                    Body =
                        @"01.07.2015 12:00 Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
                            Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. 
                            Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. 
                            Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
                }
            };

            _article2 = new ArticleContainer
            {
                Guid = _header2.Guid,
                Header = _header2,
                RssSource = _rssSource,
                Body = new BodyContainer
                {
                    HasPicture = false,
                    Body =
                        @"02.07.2015 12:00 Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
                            Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. 
                            Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. 
                            Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
                }
            };

            var contentStorageMock = new Mock<IContentStorage>();
            contentStorageMock.Setup(contentStorage => contentStorage.SaveArticle(It.IsAny<ArticleContainer>()))
                .Returns(true);
            contentStorageMock.Setup(contentStorage => contentStorage.GetArticleByUrl("http://lenta.ru/loren_ipsum_1"))
                .Returns(() =>
                {
                    var task = new Task<ArticleContainer>(() => _article1);
                    task.Start();
                    return task;
                });

            contentStorageMock.Setup(contentStorage => contentStorage.GetArticleByUrl("http://lenta.ru/loren_ipsum_2"))
                .Returns(() =>
                {
                    var task = new Task<ArticleContainer>(() => _article2);
                    task.Start();
                    return task;
                });


            contentStorageMock.Setup(contentStorage => contentStorage.GetArticleByGuid(It.IsAny<Guid>()))
                .Returns((Guid guid) =>
                {
                    ArticleContainer article = null;
                    if (guid == _header1.Guid)
                        article = _article1;
                    else if (guid == _header2.Guid)
                        article = _article2;
                    var task = new Task<ArticleContainer>(() => article);
                    task.Start();
                    return task;
                });

            contentStorageMock.Setup(contentStorage => contentStorage.GetArticlesBySource(It.IsAny<RssSource>()))
                .Returns(() => new List<ArticleContainer> {_article1, _article2});


            var parserProviderMock = new Mock<IParserProvider>();

            parserProviderMock.Setup(parserProvider => parserProvider.GetArticleBody(It.IsAny<Guid>()))
                .Returns((Guid guid) =>
                {
                    ArticleContainer article = null;
                    if (guid == _header1.Guid)
                        article = _article1;
                    else if (guid == _header2.Guid)
                        article = _article2;

                    return article;
                });

            _controller = new SourcesController(rssSourcesMock.Object, newsProviderMock.Object,
                contentStorageMock.Object, parserProviderMock.Object);
        }

        [TestMethod]
        public void Get()
        {
            //Act 
            var asyncResult = _controller.Get();
            asyncResult.Wait();

            Assert.IsNotNull(asyncResult);
            asyncResult.Wait();
            var res = asyncResult.Result;
            var result = res as OkNegotiatedContentResult<RssSources>;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);
            Assert.IsTrue(result.Content.Sources.Contains(_rssSource));
        }

        [TestMethod]
        public void Post()
        {
            //Arrange 
            var jSon = @"{
                        'page': '1',
                        'refresh': 'true',
                        'sources': [ {'sitename' : 'Lenta.ru'} ]}";

            var jsonRequest = JObject.Parse(jSon);
            // Act
            var asyncResult = _controller.Post(jsonRequest);
            asyncResult.Wait();
            var res = asyncResult.Result;
            var result = res as OkNegotiatedContentResult<NewsHeaders>;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);
            Assert.IsTrue(result.Content.TotalArticlesCount == 2);
            Assert.IsTrue(result.Content.Headers.Contains(_header1));
            Assert.IsTrue(result.Content.Headers.Contains(_header2));
        }

        [TestMethod]
        public void GetArticle()
        {
            //Act
            var asyncResult1 = _controller.GetArticle(_header1.Guid);
            var asyncResult2 = _controller.GetArticle(_header2.Guid);
            var asyncResult3 = _controller.GetArticle(new Guid());
            asyncResult1.Wait();
            asyncResult2.Wait();
            asyncResult3.Wait();

            //Assert
            var res1 = asyncResult1.Result;
            var result1 = res1 as OkNegotiatedContentResult<BodyContainer>;
            var res2 = asyncResult2.Result;
            var result2 = res2 as OkNegotiatedContentResult<BodyContainer>;
            var res3 = asyncResult3.Result;
            var result3 = res3 as NotFoundResult;

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result1.Content);
            Assert.IsTrue(result1.Content.Body == _article1.Body.Body);

            Assert.IsNotNull(result2);
            Assert.IsNotNull(result2.Content);
            Assert.IsTrue(result2.Content.Body == _article2.Body.Body);

            Assert.IsNotNull(result3);
        }

        [TestMethod]
        public void GetLogo()
        {
            //Act
            var asyncResult = _controller.GetLogo(_header1.Guid.ToString());
            asyncResult.Wait();
            var res = asyncResult.Result;
            //Assert
            Assert.IsNotNull(res);
            Assert.IsTrue(res.StatusCode == HttpStatusCode.OK);
            Assert.IsNotNull(res.Content);
            Assert.IsTrue(res.Content is StreamContent);
            var content = res.Content as StreamContent;
            var task = content.ReadAsByteArrayAsync();
            task.Wait();
            var bytes = task.Result;
            Assert.IsTrue(bytes.Length == _logo.Length);
            Assert.IsTrue(bytes.SequenceEqual(_logo));
        }

        [TestMethod]
        public void GetEnclosure()
        {
            //Act
            var asyncResult1 = _controller.GetEnclosure(_header1.Guid.ToString());
            asyncResult1.Wait();
            var asyncResult2 = _controller.GetEnclosure(_header2.Guid.ToString());
            asyncResult2.Wait();

            var res1 = asyncResult1.Result;
            var res2 = asyncResult2.Result;
            //Assert
            Assert.IsNotNull(res1);
            Assert.IsTrue(res1.StatusCode == HttpStatusCode.OK);
            Assert.IsNotNull(res1.Content);
            Assert.IsTrue(res1.Content is StreamContent);
            var content = res1.Content as StreamContent;
            var task = content.ReadAsByteArrayAsync();
            task.Wait();
            var bytes = task.Result;
            Assert.IsTrue(bytes.Length == _enclosure.Length);
            Assert.IsTrue(bytes.SequenceEqual(_enclosure));

            Assert.IsNotNull(res2);
            Assert.IsTrue(res2.StatusCode == HttpStatusCode.NotFound);
        }
    }
}