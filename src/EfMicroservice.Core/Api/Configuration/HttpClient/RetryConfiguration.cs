using System.Collections.Generic;

namespace EfMicroservice.Core.Api.Configuration.HttpClient
{
    public class RetryConfiguration
    {
        public RetryRequestConfiguration Read { get; set; }

        public RetryRequestConfiguration Write { get; set; }
    }
}