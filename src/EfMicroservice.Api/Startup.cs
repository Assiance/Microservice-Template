using EfMicroservice.Api.Infrastructure.Configurations;
using EfMicroservice.Api.Infrastructure.Exceptions;
using EfMicroservice.Api.Infrastructure.Extensions;
using EfMicroservice.Api.Infrastructure.Logging;
using EfMicroservice.Api.Infrastructure.Registrations;
using EfMicroservice.Application;
using EfMicroservice.Common;
using EfMicroservice.Domain;
using EfMicroservice.ExternalData;
using EfMicroservice.ExternalData.Clients;
using EfMicroservice.Persistence;
using EfMicroservice.Persistence.Contexts;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Omni.BuildingBlocks;
using Omni.BuildingBlocks.Api.Configuration.Authentication;
using Omni.BuildingBlocks.Http.Handlers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using EfMicroservice.Application.Products.Clients;

namespace EfMicroservice.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddVersionedApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            var authConfig = Configuration.GetSection("Authentication").Get<JwtConfiguration>();
            services.AddJwtAuthentication(authConfig);
            services.AddAuthorizationPolicies(authConfig);

            var serviceProvider = services.BuildServiceProvider();
            services.AddEntityFrameworkNpgsql()
                .AddDbContextPool<ApplicationDbContext>(options => options
                    .UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
                    .UseSnakeCaseNamingConvention()
                    .UseLoggerFactory(serviceProvider.GetService<ILoggerFactory>()));

            // Register Scoped Dependencies
            services.RegisterOmniBuildingBlockDependencies();
            services.RegisterCommonDependencies();
            services.RegisterApiDependencies();
            services.RegisterApplicationDependencies();
            services.RegisterDomainDependencies();
            services.RegisterPersistenceDependencies();
            services.RegisterExternalDataDependencies();

            // Register Transient Dependencies
            services.AddTransient<AppendCorrelationIdHeaderHandler>();
            services.AddTransient<AppendAuthHeaderHandler>();
            services.AddTransient<UnsuccessfulResponseHandler>();
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
            services.AddSwaggerGen();
            services.AddControllers(x =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    x.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ContractResolver =
                        new CamelCasePropertyNamesContractResolver());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            app.UseSwagger();
            app.UseSwaggerUIDocs(provider);

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
    }
}