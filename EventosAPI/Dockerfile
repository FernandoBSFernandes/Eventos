# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copiar apenas os .csproj e restaurar dependências.
# Essa camada só é invalidada quando um .csproj muda,
# reutilizando o cache do NuGet nas demais builds.
COPY ["EventosAPI/EventosAPI.csproj", "EventosAPI/"]
COPY ["Eventos.Application/Eventos.Application.csproj", "Eventos.Application/"]
COPY ["Eventos.Domain/Eventos.Domain.csproj", "Eventos.Domain/"]
COPY ["Eventos.Infrastructure/Eventos.Infrastructure.csproj", "Eventos.Infrastructure/"]
RUN dotnet restore "EventosAPI/EventosAPI.csproj"

# 2. Copiar o restante do código-fonte e compilar.
# Essa camada é invalidada quando qualquer fonte muda,
# mas reutiliza o cache do restore acima.
COPY . .
RUN dotnet build "EventosAPI/EventosAPI.csproj" \
        -c $BUILD_CONFIGURATION \
        --no-restore

# 3. Publicar reutilizando os artefatos do build anterior.
# --no-build evita recompilar o que já foi compilado na camada acima.
RUN dotnet publish "EventosAPI/EventosAPI.csproj" \
        -c $BUILD_CONFIGURATION \
        -o /app/publish \
        --no-build \
        /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventosAPI.dll"]
