using System;
using System.ComponentModel.DataAnnotations;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;

namespace EfMicroservice.Api.Infrastructure.Exceptions
{
    public interface IErrorResultConverter
    {
        ErrorResult GetError(BaseException exception);

        ErrorResult GetError(System.ComponentModel.DataAnnotations.ValidationException exception);

        ErrorResult GetError(FluentValidation.ValidationException exception);

        ErrorResult GetError(HttpCallException exception);

        ErrorResult GetError(Exception exception);
    }
}