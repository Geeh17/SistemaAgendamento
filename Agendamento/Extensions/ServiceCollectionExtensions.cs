using System.Text;
using Agendamento.Models;
using Agendamento.Repositories;
using Agendamento.Services;
using Agendamento.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Agendamento.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra os repositórios e serviços da aplicação.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITarefaRepository, TarefaRepository>();
            services.AddScoped<ITarefaService, TarefaService>();

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

            return services;
        }

        /// <summary>
        /// Configura o Swagger/OpenAPI com metadados descritivos da API e suporte a
        /// autenticação via JWT ("Authorize" no Swagger UI).
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sistema de Agendamento API",
                    Version = "v1",
                    Description = "API REST para gerenciamento de agendamentos, compromissos e tarefas."
                });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Informe o token no formato: Bearer {seu token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });

            return services;
        }

        /// <summary>
        /// Configura a política de CORS usada pelo front-end.
        /// </summary>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            return services;
        }

        /// <summary>
        /// Configura autenticação JWT Bearer a partir da seção "Jwt" do appsettings.json.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection(JwtSettings.SectionName);
            services.Configure<JwtSettings>(jwtSection);

            var jwtSettings = jwtSection.Get<JwtSettings>()
                ?? throw new InvalidOperationException("A seção 'Jwt' não foi configurada em appsettings.json.");

            if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
            {
                throw new InvalidOperationException(
                    "A chave 'Jwt:Key' precisa estar configurada com no mínimo 32 caracteres (256 bits).");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
