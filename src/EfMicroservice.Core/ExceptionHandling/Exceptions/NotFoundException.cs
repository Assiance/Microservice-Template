using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Core.ExceptionHandling.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException() : base(ErrorCode.KeyNotFoundException)
        {
        }

        public NotFoundException(string message) : base(message, ErrorCode.KeyNotFoundException)
        {
        }
    }
}
