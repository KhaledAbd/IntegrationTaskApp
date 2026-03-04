# GitHub Integration Task App

This project is a comprehensive application for integrating with GitHub, featuring an API, a background processing service, and a frontend monitoring interface.

## Architecture & Components

The application consists of multiple cohesive components orchestrated via Docker Compose. The architecture is designed to decoupling the front-end, the API, and background processing of GitHub data.

1. **GitHubIntegrationApi (`src/GitHubIntegrationApi`)**
   - An ASP.NET Core Web API that handles incoming requests from the frontend.
   - Runs on port `8080` internally.
   - Depends on the `GitHubIntegrationService` (WCF Service) to fetch and process GitHub data.
   - Uses Serilog for logging output to console and file (`Logs/api-log-.txt`).

2. **GitHubIntegrationService (`src/GitHubIntegrationService`)**
   - An ASP.NET Core worker service that processes GitHub-related background tasks and integrations.
   - Designed for background processing.
   - Exposes a WCF Service endpoint at `/GitHubService.svc`.
   - Uses Serilog for logging output to console and file (`Logs/log-.txt`).

3. **github-repository-monitor (`src/github-repository-monitor`)**
   - An Angular-based frontend application for monitoring GitHub repositories.
   - Exposed on port `4200` via Nginx.
   - Communicates with the `GitHubIntegrationApi` to fetch data and status.

4. **github-pusher (`src/github-pusher`)**
   - A dedicated container service handling pushing operations to GitHub repositories.

## Authentication with GitHub

Authentication with the GitHub API is handled using a **Personal Access Token (PAT)**. 

- The `GitHubIntegrationService` requires a GitHub token to authenticate its requests to the GitHub API. This is configured in the `src/GitHubIntegrationService/appsettings.json` file under the `"GitHub:Token"` key or via environment variables.
- The `github-pusher` service also requires a GitHub token, which is passed as the `GITHUB_TOKEN` environment variable in the `docker-compose.yml` file.

**Note on Security:** In a production environment, hardcoding tokens in `appsettings.json` or `docker-compose.yml` is not recommended. Instead, you should use secure secret management solutions like Azure Key Vault, AWS Secrets Manager, or Docker Secrets.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for local development outside of Docker).
- [Node.js](https://nodejs.org/) & Angular CLI (for local UI development).
- Visual Studio 2022 (optional, but recommended as the solution uses `.sln` and `.dcproj`).

## How to Run the Application

### Method 1: Using Docker Compose (Recommended)

The easiest way to get the entire stack running is via Docker Compose. This ensures all services, dependencies, and network configurations are set up automatically.

1. Clone the repository and navigate to the project root directory.
2. Build and start all services using Docker Compose:
   ```bash
   docker-compose up -d --build
   ```
3. Once the containers are healthy:
   - Access the Angular UI at: [http://localhost:4200](http://localhost:4200)
   - Access the API at: [http://localhost:8080](http://localhost:8080)

To view the logs of the services, run:
```bash
docker-compose logs -f
```

To stop the services, run:
```bash
docker-compose down
```

### Method 2: Local Development (Visual Studio 2022)

If you prefer to run and debug the services locally using Visual Studio:

1. Open `GitHubIntegration.sln` in Visual Studio 2022.
2. Ensure you have the required prerequisites installed (.NET 8 SDK, Node.js).
3. Set `docker-compose` as the startup project to run all services simultaneously using Visual Studio's Docker integration.
4. Alternatively, to run without Docker, configure multiple startup projects (right-click Solution -> Properties -> Multiple startup projects) and select `GitHubIntegrationApi`, `GitHubIntegrationService`, and `github-repository-monitor` to start.

### Method 3: Running Individual Services Separately (CLI)

If you want to run the services individually without Docker or Visual Studio:

**1. Run GitHubIntegrationService:**
```bash
cd src/GitHubIntegrationService
dotnet run
```

**2. Run GitHubIntegrationApi:**
```bash
cd src/GitHubIntegrationApi
dotnet run
```

**3. Run Angular Frontend:**
```bash
cd src/github-repository-monitor
npm install
npm start
```
The frontend will be available at `http://localhost:4200`.

## Assumptions & Notes

1. **WCF Communication:** The API communicates with the background service using WCF. The URL for this service is configured in `GitHubIntegrationApi/appsettings.json` (`WcfServiceUrl`). It assumes the service is running and accessible.
2. **Environment Variables:** The `docker-compose.yml` relies on environment variables set up in the configuration. Ensure these are correctly set if modifying the setup.
3. **Container Health:** The API and frontend containers rely on health checks to ensure dependencies are ready before starting. The frontend depends on the API, and the API depends on the background service.
4. **Hardcoded Tokens:** As mentioned in the Authentication section, the GitHub PATs are currently placed in configuration files for demonstration/development purposes. These should be moved to a secure vault for production.
5. **CORS:** The API allows all hosts (`"AllowedHosts": "*"`) by default in the provided configuration. In production, this should be restricted to the specific origin of the frontend application.
