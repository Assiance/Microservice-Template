using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Core.ExceptionHandling.Exceptions
{
    public class AccessDeniedException : BaseException
    {
        public AccessDeniedException() : base(ErrorCode.AccessDenied)
        {
        }

        public AccessDeniedException(string message) : base(message, ErrorCode.AccessDenied)
        {
        }
    }
}
