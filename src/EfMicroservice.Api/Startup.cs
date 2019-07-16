using System;
using System.Collections.Generic;
using EfMicroservice.Api.Configurations;
using EfMicroservice.Api.Exceptions;
using EfMicroservice.Core;
using EfMicroservice.Data;
using EfMicroservice.Core.Api.Configuration.HttpClient;
using EfMicroservice.Data.Clients;
using EfMicroservice.Data.Clients.Interfaces;
using EfMicroservice.Data.Contexts;
using EfMicroservice.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EfMicroservice.Core.Api.Configuration.Authentication;

namespace EfMicroservice.Api
{
    public class Startup
    {
        private readonly Dictionary<Type, Func<IServiceCollection, IHttpClientBuilder>> _clients;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _clients = new Dictionary<Type, Func<IServiceCollection, IHttpClientBuilder>>
            {
                { typeof(GitHaubService), services => services.AddHttpClient<IGitHaubService, GitHaubService>() }
            };
            //test
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddVersionedApiExplorer(o => { 
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var authConfig = Configuration.GetSection("Authentication").Get<JwtConfiguration>();
            services.AddJwtAuthentication(authConfig);
            services.AddAuthorizationPolicies(authConfig);

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationDbContext>(options => options
                    .UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
                    .UseLoggerFactory(services.BuildServiceProvider()
                        .GetService<ILoggerFactory>()))
                .BuildServiceProvider();

            // Register Scoped Dependencies
            services.RegisterApiDependencies();
            services.RegisterDomainDependencies();
            services.RegisterDataDependencies();
            services.RegisterCoreDependencies();

            // Register Transient Dependencies
            // Register Singleton Dependencies
            services.AddSingleton<IErrorResultConverter, ErrorResultConverter>();
            
            var httpClientPoliciesSection = Configuration.GetSection("HttpClientPolicies");
            services.Configure<List<HttpClientPolicy>>(httpClientPoliciesSection);

            var policies = httpClientPoliciesSection.Get<List<HttpClientPolicy>>();
            services.RegisterClients(policies, _clients);

            services.AddApiVersioning();
            services.AddSwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            ConfigureCors(app);

            app.UseSwagger();
            app.UseSwaggerUIDocs(provider);
            app.UseAuthentication();
            app.UseLoggingMiddleware();
            app.UseExceptionHandlingMiddleware();
            app.UseHttpsRedirection();
            app.UseMvc();
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
