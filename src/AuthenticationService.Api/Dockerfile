FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AuthenticationService.Api/AuthenticationService.Api.csproj", "AuthenticationService.Api/"]
COPY ["AuthenticationService.Core/AuthenticationService.Core.csproj", "AuthenticationService.Core/"]
COPY ["AuthenticationService.Infrastructure/AuthenticationService.Infrastructure.csproj", "AuthenticationService.Infrastructure/"]
RUN dotnet restore "AuthenticationService.Api/AuthenticationService.Api.csproj"
COPY . .
WORKDIR "/src/AuthenticationService.Api"
RUN dotnet build "AuthenticationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthenticationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/api/health/live || exit 1

ENTRYPOINT ["dotnet", "AuthenticationService.Api.dll"]
