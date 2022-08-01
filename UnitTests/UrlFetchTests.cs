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
            var md5Hash = UrlFetcher.GetMd5HashForUrl(url).Result;

            // assert
            Assert.Equal("38AF87976F5D5DA39D6E25136BDDE320", md5Hash);

        }
    }
}
