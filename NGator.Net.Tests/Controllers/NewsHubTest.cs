using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using NGator.Net.Controllers;
using NGator.Net.Models;

namespace NGator.Net.Tests.Controllers
{
    [TestClass]
    public class NewsHubTest
    {
        private readonly RssSource _rssSource = new RssSource
        {
            SiteName = "Lenta.ru",
            Url = "http://lenta.ru/rss"
        };

        private byte[] _enclosure;
        private NewsHeader _header1;
        private NewsHeader _header2;
        private NewsHub _hub;
        private byte[] _logo;

        [TestInitialize]
        public void Init()
        {
            // Arrange
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
            newsProviderMock.Setup(
                newsProvider => newsProvider.GetNewsFromSingleSource(It.IsAny<RssSource>(), It.IsAny<bool>())).Returns(
                    (RssSource src, bool refresh) =>
                    {
                        var task = new Task<NewsHeaders>(() => new NewsHeaders
                        {
                            TotalArticlesCount = 2,
                            Headers = new List<NewsHeader> {_header1, _header2}
                        });
                        task.Start();
                        return task;
                    });
            _hub = new NewsHub(newsProviderMock.Object, rssSourcesMock.Object);
        }

        [TestMethod]
        public void LoadNews()
        {
            //Arrange
            var jSon = @"{
                        'page': '1',
                        'refresh': 'true',
                        'sources': [ {'sitename' : 'Lenta.ru'} ]}";
            var jObject = JObject.Parse(jSon);

            var mockClient = new Mock<IHubCallerConnectionContext<dynamic>>();
            var cmock = new Mock<IClientContract>();
            cmock.Setup(progress => progress.Report(It.IsAny<int>()))
                .Callback((int progress) => Trace.WriteLine("Progress reported: " + progress)).Verifiable();
            mockClient.Setup(mc => mc.All).Returns(cmock.Object);
            _hub.Clients = mockClient.Object;
            
            //Act
            var asyncRes = _hub.LoadNews(jObject, cmock.Object);
            asyncRes.Wait();
            var result = asyncRes.Result;

            //Assert
            Assert.IsNotNull(result);            
            Assert.IsTrue(result.TotalArticlesCount == 2);
            Assert.IsTrue(result.Headers.Contains(_header1));
            Assert.IsTrue(result.Headers.Contains(_header2));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public  interface IClientContract : IProgress<int>
        {
            
        }
    }
}