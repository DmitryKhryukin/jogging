using System.Collections.Generic;
using System.Text;
using AutoMapper;
using JoggingTracker.Api.ErrorHandling;
using JoggingTracker.Core;
using JoggingTracker.Core.Constants;
using JoggingTracker.Core.Mapping;
using JoggingTracker.Core.Services;
using JoggingTracker.Core.Services.Interfaces;
using JoggingTracker.Core.Services.WeatherService;
using JoggingTracker.DataAccess;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace JoggingTracker.Api
{
    public class Startup
    {
        private const string ApiName = "Jogging Tracker API";
        private const string ApiVersion = "v1";

        public Startup(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            
            services.AddDbContext<JoggingTrackerDataContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")
                ));
            
            services.AddIdentity<UserDb, IdentityRole>()
                .AddEntityFrameworkStores<JoggingTrackerDataContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = AppConstants.MinPasswordLength;
                
                // lockout
                options.Lockout.AllowedForNewUsers = false;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });


            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(configureOptions =>
                {
                    configureOptions.RequireHttpsMetadata = true;
                    configureOptions.SaveToken = true;
                    configureOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true
                    };
                });

            // remove default jwt claim mapping
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            
            RegisterServices(services);
            
            services.AddControllers();
            
            
            // register the swagger generator
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc(ApiVersion, new OpenApiInfo(){ Title = ApiName, Version = ApiVersion });

                x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                x.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });
            
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
            });
            
            services.AddAutoMapper(typeof(AutoMapperProfile));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            
            app.UseMiddleware<CustomExceptionMiddleware>(env.IsDevelopment());

            loggerFactory.AddSerilog();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{ApiVersion}/swagger.json", $"{ApiName} {ApiVersion}");
            });
        }
        
        private void RegisterServices(IServiceCollection services)
        {
            // settings
            WeatherServiceSettings weatherServiceSettings = new WeatherServiceSettings();
            Configuration.GetSection("WeatherServiceSettings").Bind(weatherServiceSettings);
            services.AddSingleton<WeatherServiceSettings>(weatherServiceSettings);
            
            AppSettings appSettings = new AppSettings();
            Configuration.GetSection("AppSettings").Bind(appSettings);
            services.AddSingleton<AppSettings>(appSettings);
            
            // services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRunService, RunService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IRoleService, RoleService>();
            
            // weather provider
            services.AddScoped<IWeatherProvider, DarkSkyWeatherProvider>();
        }
    }
}