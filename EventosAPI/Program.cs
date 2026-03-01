using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy
                        .WithOrigins("https://fernandobsfernandes.github.io/ConvidadosCasamentoPage/", "http://127.0.0.1:5500")
                        .WithMethods("POST", "GET")
                        .WithHeaders("Content-Type");
                });
            });

            // Garante que os controllers do assembly EventosAPI
            // são sempre carregados, mesmo quando invocado por testhost
            builder.Services
                .AddControllers()
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
                options.UseInlineDefinitionsForEnums();
                // Inclui o XML de documentação se existir
                var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            // Register DbContext
            builder.Services.AddDbContext<Eventos.Infrastructure.Data.EventosDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                options.UseLoggerFactory(loggerFactory);
                options.EnableDetailedErrors();

                if (builder.Environment.IsDevelopment())
                    options.EnableSensitiveDataLogging();
            });

            // Register DDD projects services
            builder.Services.AddScoped<Eventos.Application.Interfaces.IEventoService, Eventos.Application.Services.EventoService>();
            builder.Services.AddScoped<Eventos.Domain.Repositories.IEventoRepository, Eventos.Infrastructure.Repositories.EventoRepository>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<Eventos.Infrastructure.Data.EventosDbContext>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors(CorsPolicyName);

            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
