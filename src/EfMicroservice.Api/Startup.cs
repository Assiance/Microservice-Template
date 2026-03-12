using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using EfMicroservice.Api.Infrastructure.Configurations;
using EfMicroservice.Api.Infrastructure.Exceptions;
using EfMicroservice.Api.Infrastructure.Extensions;
using EfMicroservice.Api.Infrastructure.Logging;
using EfMicroservice.Api.Infrastructure.Registrations;
using EfMicroservice.Application;
using EfMicroservice.Domain;
using EfMicroservice.ExternalData;
using EfMicroservice.Persistence;
using EfMicroservice.Persistence.Contexts;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks;
using Omni.BuildingBlocks.Api.Configuration.Authentication;
using Omni.BuildingBlocks.Http.Handlers;
using Omni.BuildingBlocks.Observability;
using Scalar.AspNetCore;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EfMicroservice.Application.Orders.Consumers;
using EfMicroservice.Application.Products.Clients;
using MassTransit;

namespace EfMicroservice.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            })
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            var authConfig = Configuration.GetSection("Authentication").Get<JwtConfiguration>();
            services.AddJwtAuthentication(authConfig);
            services.AddAuthorizationPolicies(authConfig);

            services.AddDbContextPool<ApplicationDbContext>(options => options
                .UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention()
                .UseLoggerFactory(services.BuildServiceProvider().GetService<ILoggerFactory>()));

            // OpenTelemetry
            services.AddOmniOpenTelemetry(Configuration, "EfMicroservice");

            // Health Checks
            services.AddHealthChecks()
                .AddNpgSql(
                    Configuration.GetConnectionString("DefaultConnection"),
                    name: "postgresql",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "ready", "db" })
                .AddDbContextCheck<ApplicationDbContext>(
                    name: "efcore",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "ready" });

            // MassTransit + RabbitMQ
            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCancellationConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConfig = Configuration.GetSection("RabbitMq");
                    cfg.Host(rabbitMqConfig["Host"], rabbitMqConfig["VirtualHost"], h =>
                    {
                        h.Username(rabbitMqConfig["Username"]);
                        h.Password(rabbitMqConfig["Password"]);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });

            // Register Scoped Dependencies
            services.RegisterOmniBuildingBlockDependencies();
            services.RegisterApiDependencies();
            services.RegisterApplicationDependencies();
            services.RegisterDomainDependencies();
            services.RegisterPersistenceDependencies(Configuration);
            services.RegisterExternalDataDependencies();

            // Register Transient Dependencies
            services.AddTransient<AppendCorrelationIdHeaderHandler>();
            services.AddTransient<AppendAuthHeaderHandler>();
            services.AddTransient<UnsuccessfulResponseHandler>();
            services.AddTransient<HttpClient>();
            services.AddTransient<LoggingMiddleware>();
            services.AddTransient<AddCorrelationIdToHeaderMiddleware>();
            services.AddTransient<ExceptionHandlingMiddleware>();
            // Register Singleton Dependencies
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IErrorResultConverter, ErrorResultConverter>();

            // Register HttpClients
            services.AddGitHaubClient(Configuration);

            services.AddAccessTokenProvider();
            services.AddSwagger();
            services.AddControllers(x =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    x.Filters.Add(new AuthorizeFilter(policy));
                });

            services.AddFluentValidationAutoValidation();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            ConfigureCors(app);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseLoggingMiddleware();
            app.UseAddCorrelationIdToHeaderMiddleware();
            app.UseExceptionHandlingMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapOpenApi();
                endpoints.MapScalarApiReference("/docs", o =>
                {
                    o.Title = "Microservice API";
                });

                // Liveness — process is alive
                endpoints.MapHealthChecks("/hc/live", new HealthCheckOptions
                {
                    Predicate = _ => false,
                    ResponseWriter = WriteHealthResponse
                });

                // Readiness — all dependencies healthy
                endpoints.MapHealthChecks("/hc/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = WriteHealthResponse
                });

                // Full — all checks with detail
                endpoints.MapHealthChecks("/hc/full", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = WriteHealthResponse
                });

                // Prometheus metrics scraping endpoint
                endpoints.MapPrometheusScrapingEndpoint("/metrics");
            });
        }

        private void ConfigureCors(IApplicationBuilder app)
        {
            var allowedOrigins = new List<string>();
            Configuration.GetSection("AllowedHosts").Bind(allowedOrigins);
            app.UseCors(builder => builder.WithOrigins(allowedOrigins.ToArray())
                .AllowAnyMethod()
                .AllowAnyHeader());
        }

        private static Task WriteHealthResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                totalDurationMs = report.TotalDuration.TotalMilliseconds,
                entries = report.Entries
            }, new JsonSerializerOptions { WriteIndented = true });
            return context.Response.WriteAsync(result);
        }
    }
}
