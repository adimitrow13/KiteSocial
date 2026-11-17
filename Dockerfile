# ==============================================================================
# Multi-Stage Production Dockerfile for KiteSocial.API (.NET 10)
# ==============================================================================

# ------------------------------------------------------------------------------
# Stage 1: Build & Publish
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file separately to optimize Docker layer caching on restore
COPY ["KiteSocial.API.csproj", "./"]
RUN dotnet restore "KiteSocial.API.csproj"

# Copy remaining source code and publish in Release mode
COPY . .
RUN dotnet publish "KiteSocial.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ------------------------------------------------------------------------------
# Stage 2: Final Minimal Runtime Image
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Standard container port for .NET 8+
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published binaries from build stage
COPY --from=build /app/publish .

# Run as non-root user for security hardening
USER $APP_UID

ENTRYPOINT ["dotnet", "KiteSocial.API.dll"]
