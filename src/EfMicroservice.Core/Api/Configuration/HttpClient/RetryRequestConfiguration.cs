using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Core.Api.Configuration.HttpClient
{
    public class RetryRequestConfiguration
    {
        public IEnumerable<int> IntervalsMs { get; set; }

        public IEnumerable<string> HttpStatusCodes { get; set; }
    }
}
