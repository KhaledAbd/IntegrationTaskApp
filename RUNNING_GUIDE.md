# Running Guide

This guide provides detailed instructions on how to configure and run the GitHub Integration Task App.

## Prerequisites

- **Docker Desktop**: [Install Docker Desktop](https://www.docker.com/products/docker-desktop)
- **.NET 8 SDK**: [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js & npm**: [Download Node.js](https://nodejs.org/)
- **Angular CLI**: Install globally via `npm install -g @angular/cli`

---

## Configuration

### 1. GitHub Personal Access Token (PAT)

The application requires a GitHub PAT to interact with the GitHub API.

> [!IMPORTANT]
> **Token Scope Clarification**:
> The provided token in `appsettings.Development.json` is a **read-only** token scoped specifically to the `KhaledAbd/IntegrationTaskApp` repository. It does **not** have permissions for any other private or public repositories.

To use your own repository, you **must** generate your own PAT:

1. Go to [GitHub Settings > Developer settings > Personal access tokens > Fine-grained tokens](https://github.com/settings/tokens?type=beta).
2. Click **Generate new token**.
3. Give it a name and select your repository under **Repository access**.
4. In **Permissions**, grant `Metadata: Read-only` (minimum required) and `Contents: Read-only` if needed.
5. Copy the token.

### 2. Customizing Repository & Settings

You can easily point the app to any GitHub repository.

#### Option A: Docker Compose (via `.env`)

Create/edit the `.env` file in the project root:

```env
GITHUB_TOKEN=your_new_token
REPO_OWNER=your_username
REPO_NAME=your_repository
```

#### Option B: Local Development (via `appsettings.json`)

Edit `src/GitHubIntegrationService/appsettings.json` (or `appsettings.Development.json`):

```json
{
  "GitHub": {
    "Token": "your_new_token",
    "RepoUrl": "https://api.github.com/repos/YOUR_OWNER/YOUR_REPO/commits"
  }
}
```

---

## How to Run

### Method 1: Docker Compose (Recommended)

This starts the API, Background Service, and Frontend Monitor in containers.

1. Open a terminal in the project root.
2. Run: `docker-compose up -d --build`
3. Access the UI: [http://localhost:4200](http://localhost:4200)
4. Access the API: [http://localhost:8080](http://localhost:8080)

### Method 2: Local Development (CLI)

Run each component manually for easier debugging.

1. **Start Background Service**:

   ```bash
   cd src/GitHubIntegrationService
   dotnet run
   ```

2. **Start API**:

   ```bash
   cd src/GitHubIntegrationApi
   dotnet run
   ```

3. **Start Frontend**:

   ```bash
   cd src/github-repository-monitor
   npm install
   npm start
   ```

---

## Troubleshooting

- **Connection Refused**: Ensure the `GitHubIntegrationService` is running before starting the `GitHubIntegrationApi`, as the API depends on the service's WCF endpoint.
- **Unauthorized (401/403)**: Double-check your `GITHUB_TOKEN` and ensure it has access to the specified repository.
- **Port Conflicts**: Ensure ports `8080` (API) and `4200` (UI) are not being used by other applications.
