using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;

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
                        .WithOrigins(
                            "https://fernandobsfernandes.github.io",
                            "file://"
                        )
                        .WithMethods("POST", "GET", "OPTIONS")
                        .WithHeaders("Content-Type", "Accept");
                });
            });

            // Garante que os controllers do assembly EventosAPI
            // são sempre carregados, mesmo quando invocado por testhost
            builder.Services
                .AddControllers()
                .PartManager.ApplicationParts.Add(
                    new AssemblyPart(typeof(Program).Assembly)
                );

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Inclui o XML de documentação se existir
                var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            // Register DbContext
            builder.Services.AddDbContext<Eventos.Infrastructure.Data.EventosDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            // Register DDD projects services
            builder.Services.AddScoped<Eventos.Application.Interfaces.IEventoService, Eventos.Application.Services.EventoService>();
            builder.Services.AddScoped<Eventos.Domain.Repositories.IEventoRepository, Eventos.Infrastructure.Repositories.EventoRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(CorsPolicyName);

            app.UseAuthorization();


            app.MapControllers();

            return app;
        }
    }
}
