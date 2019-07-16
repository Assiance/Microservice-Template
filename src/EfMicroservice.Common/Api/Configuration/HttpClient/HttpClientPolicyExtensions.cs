using System;
using System.Collections.Generic;
using System.Linq;

namespace EfMicroservice.Common.Api.Configuration.HttpClient
{
    public static class HttpClientPolicyExtensions
    {
        public static ClientConfiguration GetClient(this List<HttpClientPolicy> httpClientPolicies, Type clientClassType)
        {
            return httpClientPolicies.SelectMany(x => x.Clients).First(y => y.Namespace == clientClassType.FullName);
        }
    }
}
