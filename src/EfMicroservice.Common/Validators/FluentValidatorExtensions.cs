using FluentValidation;
using System;

namespace EfMicroservice.Common.Validators
{
    public static class FluentValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> IsUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute)).WithMessage("The Url field is not a valid fully-qualified http, https, or ftp URL.");
        }
    }
}
