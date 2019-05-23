using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EfMicroservice.Api.Exceptions
{
    public class ErrorResult
    {
        public ErrorResult(Error error)
        {
            Error = error;
        }

        public Error Error { get; }
    }
}
