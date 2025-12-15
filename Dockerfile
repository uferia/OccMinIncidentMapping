# Multi-stage Dockerfile for .NET 8 ASP.NET Core application
# Optimized for Google Cloud Run

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["OccMinIncidentMapping/OccMinIncidentMapping.csproj", "OccMinIncidentMapping/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]

# Restore dependencies
RUN dotnet restore "OccMinIncidentMapping/OccMinIncidentMapping.csproj"

# Copy remaining source code
COPY . .

# Build application
RUN dotnet build "OccMinIncidentMapping/OccMinIncidentMapping.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "OccMinIncidentMapping/OccMinIncidentMapping.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Set non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port (Cloud Run requires port 8080)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "OccMinIncidentMapping.dll"]
