using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using EfMicroservice.Application;
using EfMicroservice.Application.Products.Clients;
using EfMicroservice.Common;
using EfMicroservice.Domain;
using EfMicroservice.ExternalData;
using EfMicroservice.ExternalData.Clients;
using EfMicroservice.Function.Api;
using EfMicroservice.Function.Api.Infrastructure.Exceptions;
using EfMicroservice.Function.Api.Infrastructure.Logging;
using EfMicroservice.Function.Api.Infrastructure.Registrations;
using EfMicroservice.Persistence;
using EfMicroservice.Persistence.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks;
using Omni.BuildingBlocks.Http.Handlers;
using Omni.BuildingBlocks.Logging;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]
namespace EfMicroservice.Function.Api
{
    public class Startup : FunctionsStartup
    {
        public IConfiguration Configuration { get; }

        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

    public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.With<TimestampUtcEnricher>()
                .CreateLogger();

            services.AddLogging(lb => lb.AddSerilog(Log.Logger));

            services.AddEntityFrameworkNpgsql()
                .AddDbContextPool<ApplicationDbContext>(options => options
                    .UseNpgsql(Configuration.GetValue<string>("DbConnectionString"))
                    .UseSnakeCaseNamingConvention());

            // Register Scoped Dependencies
            services.RegisterOmniBuildingBlockDependencies();
            services.RegisterCommonDependencies();
            services.RegisterFunctionApiDependencies();
            services.RegisterApplicationDependencies();
            services.RegisterDomainDependencies();
            services.RegisterPersistenceDependencies();
            services.RegisterExternalDataDependencies();

            // Register Transient Dependencies
            services.AddTransient<AppendCorrelationIdHeaderHandler>();
            services.AddTransient<AppendAuthHeaderHandler>();
            services.AddTransient<UnsuccessfulResponseHandler>();
            services.AddTransient<UnsuccessfulResponseHandler>();
            services.AddTransient<LoggingMiddleware>();
            services.AddTransient<AddCorrelationIdToHeaderMiddleware>();
            services.AddTransient<ExceptionHandlingMiddleware>();

            // Register Singleton Dependencies
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IErrorResultConverter, ErrorResultConverter>();

            // Register HttpClients
            services.AddGitHaubClient(Configuration);

            //services.AddSwagger();
            //services.AddSwaggerGen();
        }
    }
}