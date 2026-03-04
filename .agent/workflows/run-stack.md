---
description: Build and run the full stack using Docker Compose
---

# Running the Full Stack

This workflow helps you build and start all project services (API, WCF Service, and Angular Frontend).

## Steps

1. **Stop existing containers (optional)**
   ```powershell
   docker-compose down
   ```

2. **Build and start the services**
// turbo
   ```powershell
   docker-compose up --build -d
   ```

3. **Verify the services are running**
   ```powershell
   docker-compose ps
   ```

4. **Access the applications**
- **API (Swagger)**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Frontend**: [http://localhost:4200](http://localhost:4200)
- **WCF Service**: [http://localhost:8081/GitHubService.svc](http://localhost:8081/GitHubService.svc) (Note: Check port in docker-compose.yml if different)

## Troubleshooting
- If the build fails, check the logs: `docker-compose logs -f`
- Ensure no other services are using ports 8080 or 4200.
