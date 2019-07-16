using System;
using System.ComponentModel.DataAnnotations;
using EfMicroservice.Common.ExceptionHandling.Exceptions;

namespace EfMicroservice.Api.Infrastructure.Exceptions
{
    public interface IErrorResultConverter
    {
        ErrorResult GetError(BaseException exception);

        ErrorResult GetError(ValidationException exception);

        ErrorResult GetError(FluentValidation.ValidationException exception);

        ErrorResult GetError(HttpCallException exception);

        ErrorResult GetError(Exception exception);
    }
}