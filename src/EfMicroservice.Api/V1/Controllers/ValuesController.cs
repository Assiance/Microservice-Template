using System;
using System.Collections.Generic;
using System.Net.Http;
using EfMicroservice.Core.ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Api.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger;

        public ValuesController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(ValuesController));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(404)]
        public ActionResult<IEnumerable<string>> Get()
        {
            _logger.LogError("Logging Things!!!");

            throw new BadRequestException("WRONG");

            return new [] { "value1", "value2" };
        }

        [HttpGet("{id}", Name = "GetValueById")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        public IActionResult Post([FromBody] string value)
        {
            return CreatedAtRoute( "GetValueById", new { id = 0 }, value );
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public void Delete(int id)
        {
        }
    }
}
