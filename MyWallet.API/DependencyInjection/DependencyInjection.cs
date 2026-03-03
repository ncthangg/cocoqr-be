using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyWallet.API.Configurations;
using MyWallet.API.Validators;
using MyWallet.Domain.Constants;
using System.Text;
using System.Text.Json;

namespace MyWallet.API.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.JWTConfig(configuration);
            services.ConfigCors(configuration);
            services.ConfigRoute();
            services.ConfigAuthentication(configuration);
            services.ConfigAuthorization();
            services.ConfigSwagger();
            services.AddLoggings();
            services.ConfigController();
        }
        private static void JWTConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(option =>
            {
                JwtConfiguration jwt = new()
                {
                    SecretKey = configuration.GetValue<string>(Jwt.SecretKeyConfigPath) ?? string.Empty,
                    Issuer = configuration.GetValue<string>(Jwt.IssuerConfigPath) ?? string.Empty,
                    Audience = configuration.GetValue<string>(Jwt.AudienceConfigPath) ?? string.Empty,
                    AccessTokenExpirationMinutes = configuration.GetValue<int>(Jwt.AccessTokenExpirationConfigPath),
                    RefreshTokenExpirationDays = configuration.GetValue<int>(Jwt.RefreshTokenExpirationConfigPath),
                };
                JwtConfigurationValidator.ValidateAndReturn(jwt);
                return jwt;
            });
        }
        private static void ConfigCors(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddCors(options =>
            {
                var allowedOrigins = configuration.GetSection(ConfigOrigins.AllowedOrigins).Get<string[]>() ??
                                    configuration[ConfigOrigins.AllowedOrigins]
                                    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(allowedOrigins!)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
        }
        private static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }
        private static void ConfigAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "OAuthTemp";
            })
            .AddCookie("OAuthTemp", o =>
            {
                o.Cookie.SameSite = SameSiteMode.None;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.HttpOnly = true;
                o.Cookie.Name = "GoogleOAuthTemp";
                o.LoginPath = "/api/auths/google-auth/signin";
                o.LogoutPath = "/api/auths/signout";
            })
            .AddGoogle(o =>
            {
                o.ClientId = configuration.GetValue<string>(Google.ClientIdConfigPath)!;
                o.ClientSecret = configuration.GetValue<string>(Google.ClientSecretConfigPath)!;

                o.SignInScheme = "OAuthTemp";
                o.SaveTokens = true;

                o.CallbackPath = "/signin-google";
                o.ClaimActions.MapJsonKey("urn:google:picture", "picture");

                o.AccessType = "offline";  // Request refresh token

                o.Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = context =>
                    {
                        var redirectUri = context.RedirectUri;

                        // ✅ CRITICAL: Always add prompt=select_account
                        // This FORCES Google to show account selection
                        if (!redirectUri.Contains("prompt="))
                        {
                            redirectUri += "&prompt=select_account";
                        }
                        else
                        {
                            // ✅ If prompt exists, replace it
                            redirectUri = System.Text.RegularExpressions.Regex.Replace(
                                redirectUri,
                                @"prompt=[^&]*",
                                "prompt=select_account"
                            );
                        }

                        // ✅ Add login_hint=null to clear cached user
                        if (!redirectUri.Contains("login_hint="))
                        {
                            redirectUri += "&login_hint=&account=&hd=&include_granted_scopes=true";
                        }

                        context.Response.Redirect(redirectUri);
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = context =>
                    {
                        Console.WriteLine($"Google Auth Error: {context.Failure?.Message}");
                        context.Response.Redirect("/");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.GetValue<string>(Jwt.IssuerConfigPath) ?? string.Empty,
                    ValidAudience = configuration.GetValue<string>(Jwt.AudienceConfigPath) ?? string.Empty,
                    IssuerSigningKey = new SymmetricSecurityKey((Encoding.UTF8.GetBytes(configuration.GetValue<string>(Jwt.SecretKeyConfigPath) ?? string.Empty)!)),
                    NameClaimType = "id",
                    ClockSkew = TimeSpan.Zero   // prevent silent time-related issues
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine("OnMessageReceived called");
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        Console.WriteLine($"Path: {path}, TokenRes: {accessToken}");
                        if (path.StartsWithSegments("/notificationHub"))
                        {
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("true TokenRes validated");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("false TokenRes invalid: " + context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });
        }
        private static void ConfigSwagger(this IServiceCollection services)
        {
            // config swagger
            services.AddSwaggerGen(c =>
            {
                c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
                c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
                c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "MyWallet"

                });
                // Thêm JWT Bearer Token vào Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header sử dụng scheme Bearer.",
                    Type = SecuritySchemeType.Http,
                    Name = "Authorization",
                    Scheme = "bearer"
                });
                //c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                //{
                //    Type = SecuritySchemeType.OAuth2,
                //    Flows = new OpenApiOAuthFlows
                //    {
                //        AuthorizationCode = new OpenApiOAuthFlow
                //        {
                //            AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                //            TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                //            Scopes = new Dictionary<string, string>
                //        { { "openid", "OpenID Connect" }, { "profile", "User profile" }, { "email", "User email" } }
                //        }
                //    }
                //});
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });

                // Đọc các nhận xét XML
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });
        }

        public static void ConfigAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                var defaultAuthPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
                defaultAuthPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthPolicyBuilder.Build();
            });

        }
        private static void AddLoggings(this IServiceCollection services)
        {
            services.AddLogging();
        }
        private static void ConfigController(this IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        }
        //private static void BackgroundServices(this IServiceCollection services)
        //{
        //    services.AddHostedService<VisitLogSyncWorker>();
        //    services.AddHostedService<VisitDetailLogWorker>();
        //}
    }
}
