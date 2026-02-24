using EfMicroservice.Application;
using EfMicroservice.Common;
using EfMicroservice.Domain;
using EfMicroservice.ExternalData;
using EfMicroservice.Function.Api;
using EfMicroservice.Function.Api.Infrastructure.Exceptions;
using EfMicroservice.Function.Api.Infrastructure.Logging;
using EfMicroservice.Function.Api.Infrastructure.Registrations;
using EfMicroservice.Persistence;
using EfMicroservice.Persistence.Contexts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Omni.BuildingBlocks;
using Omni.BuildingBlocks.Http.Handlers;
using Omni.BuildingBlocks.Logging;
using Serilog;
using System.IO;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.With<TimestampUtcEnricher>()
    .CreateLogger();

builder.Services.AddLogging(lb => lb.AddSerilog(Log.Logger));

builder.Services.AddDbContextPool<ApplicationDbContext>(options => options
    .UseNpgsql(configuration.GetValue<string>("DbConnectionString"))
    .UseSnakeCaseNamingConvention());

builder.Services.RegisterOmniBuildingBlockDependencies();
builder.Services.RegisterCommonDependencies();
builder.Services.RegisterFunctionApiDependencies();
builder.Services.RegisterApplicationDependencies();
builder.Services.RegisterDomainDependencies();
builder.Services.RegisterPersistenceDependencies();
builder.Services.RegisterExternalDataDependencies();

builder.Services.AddTransient<AppendCorrelationIdHeaderHandler>();
builder.Services.AddTransient<AppendAuthHeaderHandler>();
builder.Services.AddTransient<UnsuccessfulResponseHandler>();
builder.Services.AddTransient<LoggingMiddleware>();
builder.Services.AddTransient<AddCorrelationIdToHeaderMiddleware>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddSingleton<IErrorResultConverter, ErrorResultConverter>();

builder.Services.AddGitHaubClient(configuration);

builder.Build().Run();
