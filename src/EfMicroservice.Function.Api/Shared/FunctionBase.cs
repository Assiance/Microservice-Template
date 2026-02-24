using System;
using System.IO;
using System.Threading.Tasks;
using EfMicroservice.Function.Api.Infrastructure.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EfMicroservice.Function.Api.Shared
{
    public abstract class FunctionBase
    {
        private readonly IServiceProvider _serviceProvider;

        protected FunctionBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<T> GetRequestBodyAndValidateAsync<T>(HttpRequest request)
        {
            var requestObject = await GetJsonBodyAsync<T>(request);
            var validator = _serviceProvider.GetRequiredService<IValidator<T>>();

            if (validator == null)
                throw new ArgumentNullException($"Validator: {typeof(IValidator<T>)} not registered");

            try
            {
                validator.ValidateAndThrow(requestObject);
            }
            catch (FluentValidation.ValidationException ex)
            {
                ex.Data[ExceptionDataKeys.IsBadRequest] = true;
                throw;
            }

            return requestObject;
        }

        public async Task<T> GetJsonBodyAsync<T>(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            var requestBody = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(requestBody);
        }
    }
}
