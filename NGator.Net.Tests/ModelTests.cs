using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NGator.Net.Models;

namespace NGator.Net.Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void RssSourcesProviderTest()
        {
            var provider = new RssSourcesProvider();
            var sources = provider.GetRssSources();
        }
    }
}
