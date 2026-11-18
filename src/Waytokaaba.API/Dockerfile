# Build context: the repository root
#   docker build -f src/Waytokaaba.API/Dockerfile -t waytokaaba-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached layer)
COPY Directory.Build.props ./
COPY src/Waytokaaba.Core/Waytokaaba.Core.csproj src/Waytokaaba.Core/
COPY src/Waytokaaba.Application/Waytokaaba.Application.csproj src/Waytokaaba.Application/
COPY src/Waytokaaba.Infrastructure/Waytokaaba.Infrastructure.csproj src/Waytokaaba.Infrastructure/
COPY src/Waytokaaba.API/Waytokaaba.API.csproj src/Waytokaaba.API/
RUN dotnet restore src/Waytokaaba.API/Waytokaaba.API.csproj

COPY src/ src/
RUN dotnet publish src/Waytokaaba.API/Waytokaaba.API.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    PORT=8080
COPY --from=build /app/publish .
# Admin-uploaded photos. Mount a persistent volume here in production (Coolify: Persistent Storage -> /app/uploads).
RUN mkdir -p /app/uploads && chown $APP_UID /app/uploads
# Run as the non-root user shipped with the official .NET images
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Waytokaaba.API.dll"]
