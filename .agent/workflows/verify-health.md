---
description: Verify the status and health of all system components
---

# Verifying System Health

This workflow helps you check if all services are responsive and healthy.

## Steps

1. **Check Docker Container Status**
   ```powershell
   docker-compose ps
   ```

2. **Verify API Health Endpoint**
   ```powershell
   curl -f http://localhost:8080/health
   ```

3. **Check Service Logs for Errors**
   ```powershell
   docker-compose logs --tail=50
   ```

4. **Manual Verification**
- Open Swagger at [http://localhost:8080/swagger](http://localhost:8080/swagger)
- Check Frontend at [http://localhost:4200](http://localhost:4200)

## Common Issues
- **Container Exit**: Check logs using `docker-compose logs <service-name>`.
- **Network Issues**: Ensure the `appnet` bridge network is correctly created by Docker Compose.
