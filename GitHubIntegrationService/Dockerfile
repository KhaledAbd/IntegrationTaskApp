FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and restore
COPY . .
RUN dotnet restore "GitHubIntegrationService.csproj"

# Build
RUN dotnet build "GitHubIntegrationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GitHubIntegrationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitHubIntegrationService.dll"]
