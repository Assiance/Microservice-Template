using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EfMicroservice.Api.Exceptions
{
    public class ValidationExceptionDetailProperty
    {
        public string PropertyName { get; set; }

        public object PropertyValue { get; set; }

        public string ErrorMessage { get; set; }
    }
}
