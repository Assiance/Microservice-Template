using FluentValidation;
using System;
using System.Linq.Expressions;

namespace EfMicroservice.Common
{
    public static class ValidatorExtensions
    {
        public static void ValidatePropertyAndThrow<T>(this AbstractValidator<T> validator, T entity, params Expression<Func<T, object>>[] propertyExpressions)
        {
            var result = validator.Validate(entity, propertyExpressions);
            if (!result.IsValid)
            {
                throw new FluentValidation.ValidationException(result.Errors);
            }
        }
    }
}
