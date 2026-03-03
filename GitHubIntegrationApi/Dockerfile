FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and restore
COPY . .
RUN dotnet restore "GitHubIntegrationApi.csproj"

# Build
RUN dotnet build "GitHubIntegrationApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GitHubIntegrationApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitHubIntegrationApi.dll"]
