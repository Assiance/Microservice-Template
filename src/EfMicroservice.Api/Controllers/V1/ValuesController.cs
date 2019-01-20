using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Core.ExceptionHandling.Exceptions;
using EfMicroservice.Data.Contexts;
using EfMicroservice.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public ValuesController(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger(nameof(ValuesController));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            _logger.LogError("Logging Things!!!");

            throw new BadRequestException("WRONG");

            return Ok(await _dbContext.Values.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id}", Name = "GetValueById")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<string>> Get(Guid id)
        {
            var value = await _dbContext.Values.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);

            if (value == null)
            {
                return NotFound();
            }

            return Ok(value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            var newValue = new ValueEntity()
            {
                Name = value
            };

            var createdValue = await _dbContext.Values.AddAsync(newValue);
            await _dbContext.SaveChangesAsync();

            return CreatedAtRoute( "GetValueById", new { id = createdValue.Entity.Id }, createdValue.Entity );
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(Guid id, [FromBody] string value)
        {
            var retrievedValue = await _dbContext.Values.FindAsync(id);

            if (retrievedValue == null)
            {
                return NotFound();
            }

            retrievedValue.Name = value;

            _dbContext.Values.Update(retrievedValue);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var retrievedValue = await _dbContext.Values.FindAsync(id);

            if (retrievedValue == null)
            {
                return NotFound();
            }

            _dbContext.Values.Remove(retrievedValue);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
