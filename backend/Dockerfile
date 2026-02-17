# Backend Dockerfile - Multi-stage build for ASP.NET Core 10
# Development stage with hot reload support

# Stage 1: Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/SMS.API/SMS.API.csproj", "SMS.API/"]
COPY ["src/SMS.Application/SMS.Application.csproj", "SMS.Application/"]
COPY ["src/SMS.Domain/SMS.Domain.csproj", "SMS.Domain/"]
COPY ["src/SMS.Infrastructure/SMS.Infrastructure.csproj", "SMS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SMS.API/SMS.API.csproj"

# Copy all source code
COPY src/ .

# Build the application
WORKDIR "/src/SMS.API"
RUN dotnet build "SMS.API.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "SMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SMS.API.dll"]

# Stage 5: Development with hot reload
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /src

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 10.0.1
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy project files and restore
COPY ["src/SMS.API/SMS.API.csproj", "SMS.API/"]
COPY ["src/SMS.Application/SMS.Application.csproj", "SMS.Application/"]
COPY ["src/SMS.Domain/SMS.Domain.csproj", "SMS.Domain/"]
COPY ["src/SMS.Infrastructure/SMS.Infrastructure.csproj", "SMS.Infrastructure/"]
RUN dotnet restore "SMS.API/SMS.API.csproj"

# Copy all source (will be overridden by volume mount)
COPY src/ .

WORKDIR "/src/SMS.API"

# Use dotnet watch for hot reload
CMD ["dotnet", "watch", "run", "--no-restore", "--urls", "http://+:8080"]
