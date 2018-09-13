using System;
using System.ComponentModel.DataAnnotations;
using EfMicroservice.Core.ExceptionHandling.Exceptions;

namespace EfMicroservice.Api.Exceptions
{
    public interface IErrorResultConverter
    {
        ErrorResult GetError(BaseException exception);

        ErrorResult GetError(ValidationException exception);

        ErrorResult GetError(HttpCallException exception);

        ErrorResult GetError(Exception exception);
    }
}