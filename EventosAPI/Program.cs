using Eventos.Application.Configuration;
using Eventos.Application.Interfaces;
using Eventos.Application.Services;
using Eventos.Domain.Repositories;
using Eventos.Infrastructure.Data;
using Eventos.Infrastructure.Repositories;
using EventosAPI.Services;
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
            CreateApp(args).Run();
        }

        public static WebApplication CreateApp(string[] args)
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
                    Description = "API para gerenciamento de convidados, relatórios e administração do evento."
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

            // Register DDD projects services
            builder.Services.Configure<EventoConfiguration>(
                builder.Configuration.GetSection(EventoConfiguration.SectionName));
            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection(EmailSettings.SectionName));
            builder.Services.AddScoped<IConvidadoService, ConvidadoService>();
            builder.Services.AddScoped<IAdministracaoService, AdministracaoService>();
            builder.Services.AddScoped<IRelatorioService, RelatorioService>();
            builder.Services.AddScoped<IRelatorioEmailService, RelatorioEmailService>();
            builder.Services.AddScoped<IEventoRepository, EventoRepository>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<EventosDbContext>();
                    db.Database.Migrate();
                } 
            }

            // Configure the HTTP request pipeline.
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
