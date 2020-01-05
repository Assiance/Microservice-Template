using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;
using Polly.CircuitBreaker;

namespace EfMicroservice.Api.Infrastructure.Exceptions
{
    public interface IErrorResultConverter
    {
        ErrorResult GetError(BaseException exception);

        ErrorResult GetError(ValidationException exception);

        ErrorResult GetError(FluentValidation.ValidationException exception);

        ErrorResult GetError(HttpCallException exception);

        ErrorResult GetError(BrokenCircuitException<HttpResponseMessage> exception);

        ErrorResult GetError(DbUpdateConcurrencyException exception);

        ErrorResult GetError(DbUpdateException exception);

        ErrorResult GetError(Exception exception);
    }
}