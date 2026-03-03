---
description: Regenerate the Angular API client from the Nswag specification
---

# Updating the API Client

Use this workflow when the `GitHubIntegrationApi` endpoints change and you need to update the frontend's `ApiClient`.

## Prerequisites
- The `GitHubIntegrationApi` must be running locally or in Docker.
- Ensure the Nswag specification is accessible at the configured URL.

## Steps

1. **Navigate to the frontend directory**
   ```powershell
   cd d:\TaskBar\github-repository-monitor
   ```

2. **Run the Nswag generation script**
// turbo
   ```powershell
   npm run build:proxy
   ```

3. **Verify the changes**
- Check the generated TypeScript files in `src/app/api-client/` (or the configured output path in `nswag.json`).
- Ensure the frontend builds correctly: `npm run build`.

## Troubleshooting
- If `build:proxy` fails, verify the API is running and `nswag.json` points to the correct endpoint.
