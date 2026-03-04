# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
RUN mkdir -p /app_data/Logs /app_data/Cache && chown -R $APP_UID:$APP_UID /app_data
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/GitHubIntegrationService/GitHubIntegrationService.csproj", "src/GitHubIntegrationService/"]
RUN dotnet restore "./src/GitHubIntegrationService/GitHubIntegrationService.csproj"
COPY . .
WORKDIR "/src/src/GitHubIntegrationService"
RUN dotnet build "./GitHubIntegrationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GitHubIntegrationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "GitHubIntegrationService.dll"]