using System;
using GrupoWebBackend.DomainAdoptionsRequests.Domain.Repositories;
using GrupoWebBackend.DomainAdoptionsRequests.Domain.Services;
using GrupoWebBackend.DomainAdoptionsRequests.Persistence.Repositories;
using GrupoWebBackend.DomainAdoptionsRequests.Services;
using GrupoWebBackend.DomainAdvertisements.Domain.Repositories;
using GrupoWebBackend.DomainAdvertisements.Domain.Services;
using GrupoWebBackend.DomainAdvertisements.Persistence.Repositories;
using GrupoWebBackend.DomainAdvertisements.Services;
using GrupoWebBackend.DomainDistrict.Domain.Repositories;
using GrupoWebBackend.DomainDistrict.Domain.Services;
using GrupoWebBackend.DomainDistrict.Persistence.Repositories;
using GrupoWebBackend.DomainDistrict.Services;
using GrupoWebBackend.DomainPets.Domain.Repositories;
using GrupoWebBackend.DomainPets.Domain.Services;
using GrupoWebBackend.DomainPets.Persistence.Repositories;
using GrupoWebBackend.DomainPets.Services;
using GrupoWebBackend.DomainPublications.Domain.Repositories;
using GrupoWebBackend.DomainPublications.Domain.Services;
using GrupoWebBackend.DomainPublications.Persistence.Repositories;
using GrupoWebBackend.DomainPublications.Services;
using GrupoWebBackend.Security.Authorization.Handlers.Implementations;
using GrupoWebBackend.Security.Authorization.Handlers.Interfaces;
using GrupoWebBackend.Security.Authorization.Middleware;
using GrupoWebBackend.Security.Authorization.Settings;
using GrupoWebBackend.Security.Domain.Repositories;
using GrupoWebBackend.Security.Domain.Services;
using GrupoWebBackend.Security.Persistence.Repositories;
using GrupoWebBackend.Security.Services;
using GrupoWebBackend.Shared.Persistence.Context;
using GrupoWebBackend.Shared.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using GrupoWebBackend.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GrupoWebBackend
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
            // Add CORS Support
            services.AddCors();
            
            services.AddControllers();
            
            // Lowercase Endpoints
            services.AddRouting(options => options.LowercaseUrls = true);
            
            // Configure AppSettings object 
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));



            string hostname;
            string username;
            string password;
            
            
                // deserializeObject= new Credential();
                var client = new SecretClient(new Uri("https://keyvaultarqui.vault.azure.net/"), new DefaultAzureCredential());
                var cloudRsaKey = client.GetSecretAsync("prueba").Result.Value.Value;
                Console.WriteLine(cloudRsaKey);
                dynamic data = JObject.Parse(cloudRsaKey);
                Console.WriteLine(data.hostname);
                Console.WriteLine(data.username);
                Console.WriteLine(data.password);

                hostname = data.hostname;
                username = data.username;
                password = data.password;

                // String jsonString = JsonSerializer.Serialize(cloudRsaKey);
                // var serializeObject = JObject.Parse(jsonString);
                // Console.WriteLine(serializeObject.SelectToken("hostname").Values<string>());
                // deserializeObject.hostname = serializeObject.SelectToken("hostname").Value<String>();
                // deserializeObject.password = serializeObject.SelectToken("password").Value<String>();
                // deserializeObject.username = serializeObject.SelectToken("username").Value<String>();
                // deserializeObject    = JsonConvert.DeserializeObject<Credential>(serializeObject);
                // Console.WriteLine($"{deserializeObject.hostname}");
                // Console.WriteLine($"{deserializeObject.username}");
                // Console.WriteLine($"{deserializeObject.password}");
   
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("GrupoWebBackend-api-in-memory");

                // options.UseMySQL($"server={hostname}; user={username}; database=adoptme-mysql; password={password}; port=3306");
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "GrupoWebBackend", Version = "v1"});
                c.EnableAnnotations();
            });
            
            // DbContext
            services.AddDbContext<AppDbContext>();
            
            // Dependency Injection Configuration
            services.AddScoped<IJwtHandler, JwtHandler>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPetRepository, PetRepository>();
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<IPublicationRepository, PublicationRepository>();
            services.AddScoped<IPublicationService, PublicationService>();
            services.AddScoped<IAdoptionsRequestsRepository,AdoptionsRequestsRepository>();
            services.AddScoped<IAdoptionsRequestsService,AdoptionsRequestsService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
            services.AddScoped<IAdvertisementService, AdvertisementService>();
            services.AddScoped<IDistrictService, DistrictService>();
            services.AddScoped<IDistrictRepository, DistrictRepository>();
            
            // AutoMapper Configuration
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GrupoWebBackend v1"));
            }

            // Apply CORS Policies
            app.UseCors(p => p
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            
            // Integrate Error Handling Middleware
            app.UseMiddleware<ErrorHandlerMiddleware>();
            
            // Integrate JWT Authorization Middleware
            app.UseMiddleware<JwtMiddleware>();
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}