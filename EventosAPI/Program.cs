using Eventos.Application.Configuration;
using Eventos.Application.Interfaces;
using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Eventos.Infrastructure.Data;
using Eventos.Infrastructure.Reports;
using Eventos.Infrastructure.Repositories;
using Eventos.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using QuestPDF.Infrastructure;

namespace EventosAPI
{
    public class Program
    {
        private const string CorsPolicyName = "ProducaoPolicy";

        public static void Main(string[] args)
        {
            CreateAppAsync(args).GetAwaiter().GetResult().Run();
        }

        public static async Task<WebApplication> CreateAppAsync(string[] args)
        {
            AppContext.SetSwitch("System.Net.DisableIPv6", true);

            QuestPDF.Settings.License = LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy
                        .WithOrigins(
                            "https://fernandobsfernandes.github.io"
                        )
                        .WithMethods("POST", "GET", "OPTIONS")
                        .WithHeaders("Content-Type");
                });
            });

            builder.Services
                .AddControllers(options =>
                {
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .PartManager.ApplicationParts.Add(
                    new AssemblyPart(typeof(Program).Assembly)
                );

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Title = "Eventos API",
                    Version = "v1",
                    Description = "API para gerenciamento de convidados, relatórios e administração do evento de casamento de Fernando e Suzana Fernandes."
                });

                options.UseInlineDefinitionsForEnums();

                var assemblies = new[]
                {
                    typeof(Program).Assembly,
                    typeof(Eventos.Application.DTOs.Request.AdicionarConvidadoRequest).Assembly
                };

                foreach (var assembly in assemblies)
                {
                    var xmlFile = $"{assembly.GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                        options.IncludeXmlComments(xmlPath);
                }
            });

            builder.Services.AddDbContext<EventosDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                options.UseLoggerFactory(loggerFactory);

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            builder.Services.AddDbContext<OrigemDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("OrigemConnection"));
            });

            // Register DDD projects services
            builder.Services.Configure<EventoConfiguration>(
                builder.Configuration.GetSection(EventoConfiguration.SectionName));
            builder.Services.AddScoped<IConvidadoService, ConvidadoService>();
            builder.Services.AddScoped<IAdministracaoService, AdministracaoService>();
            builder.Services.AddScoped<IRelatorioService, RelatorioService>();
            builder.Services.AddScoped<IEventoRepository, EventoRepository>();
            builder.Services.AddScoped<IOrigemRepository, OrigemRepository>();
            builder.Services.AddScoped<IMigracaoDadosService, MigracaoDadosService>();
            builder.Services.AddScoped<RelatorioPdfStrategy>();
            builder.Services.AddScoped<RelatorioExcelStrategy>();
            builder.Services.AddScoped<IRelatorioFactory, RelatorioFactory>();

            var app = builder.Build();

#if !DEBUG
            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                try
                {
                    var destino = scope.ServiceProvider.GetRequiredService<EventosDbContext>();

                    var tentativas = 5;
                    var espera = TimeSpan.FromSeconds(5);
                    for (var i = 1; i <= tentativas; i++)
                    {
                        try
                        {
                            destino.Database.Migrate();
                            logger.LogInformation("[Startup] Migrations do EventosDbContext aplicadas com sucesso.");
                            break;
                        }
                        catch (Exception ex) when (i < tentativas)
                        {
                            logger.LogWarning(ex, "[Startup] Tentativa {Tentativa}/{Total} falhou para EventosDbContext. Aguardando {Espera}s antes de tentar novamente.", i, tentativas, espera.TotalSeconds);
                            await Task.Delay(espera);
                            espera *= 2;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Startup] Falha ao aplicar migrations do EventosDbContext após todas as tentativas.");
                }

                try
                {
                    var origem = scope.ServiceProvider.GetRequiredService<OrigemDbContext>();
                    origem.Database.Migrate();
                    logger.LogInformation("[Startup] Migrations do OrigemDbContext aplicadas com sucesso.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Startup] Falha ao aplicar migrations do OrigemDbContext.");
                }
            }
#endif

            if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

            app.UseCors(CorsPolicyName);

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
