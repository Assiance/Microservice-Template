using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EfMicroservice.Api.Exceptions
{
    public class ErrorResult
    {
        public ErrorResult(params Error[] errors)
        {
            Errors = errors;
        }

        public IEnumerable<Error> Errors { get; }
    }
}
