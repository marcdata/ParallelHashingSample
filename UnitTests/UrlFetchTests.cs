using System;
using Xunit;
using ParallelHashingSample;

namespace UnitTests
{
    public class UrlFetchTests
    {
        [Fact]
        public void Test1()
        {
            // arrange 
            var url = "http://www.google.com";

            // act 
            var md5Hash = UrlFetcher.GetMd5HashForSite(url).Result;

            // assert
            Assert.NotEqual("", md5Hash);

        }
    }
}
