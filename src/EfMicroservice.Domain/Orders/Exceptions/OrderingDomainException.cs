using System;
using System.Collections.Generic;
using System.Text;
using Omni.BuildingBlocks.ExceptionHandling;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;

namespace EfMicroservice.Domain.Orders.Exceptions
{
    public class OrderingDomainException : BaseException
    {
        public OrderingDomainException()
        { }

        public OrderingDomainException(string message)
            : base(message, ErrorCode.System)
        { }
    }
}
