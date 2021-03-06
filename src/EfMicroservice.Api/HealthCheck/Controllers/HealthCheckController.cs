﻿using EfMicroservice.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace EfMicroservice.Api.HealthCheck.Controllers
{
    [AllowAnonymous]
    [ApiVersionNeutral]
    [Route("hc")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public HealthCheckController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult Get()
        {
            var health = new
            {
                Status = "alive",
                ApplicationName = _environment.ApplicationName,
                Environment = _environment.EnvironmentName
            };

            return Ok(health);
        }

        [Authorize(Permissions.ReadMessages)]
        [HttpGet("private", Name = "privateTest")]
        [ProducesResponseType(200)]
        public IActionResult GetPrivate()
        {
            var health = new
            {
                Status = "alive",
                ApplicationName = _environment.ApplicationName,
                Environment = _environment.EnvironmentName
            };

            return Ok(health);
        }
    }
}