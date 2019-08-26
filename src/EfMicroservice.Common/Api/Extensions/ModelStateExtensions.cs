using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace EfMicroservice.Common.Api.Extensions
{
    public static class ModelStateExtensions
    {
        public static IEnumerable<string> ToFormattedErrors(this ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
        }
    }
}
