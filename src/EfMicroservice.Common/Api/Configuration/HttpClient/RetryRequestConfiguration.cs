using System.Collections.Generic;

namespace EfMicroservice.Common.Api.Configuration.HttpClient
{
    public class RetryRequestConfiguration
    {
        public IEnumerable<int> IntervalsMs { get; set; }

        public IEnumerable<string> HttpStatusCodes { get; set; }
    }
}
