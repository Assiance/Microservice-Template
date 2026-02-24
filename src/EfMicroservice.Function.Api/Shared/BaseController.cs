using System;

namespace EfMicroservice.Function.Api.Shared
{
    public abstract class BaseController : FunctionBase
    {
        protected BaseController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
