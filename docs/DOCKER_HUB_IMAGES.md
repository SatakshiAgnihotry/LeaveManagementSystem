# Docker Hub Image Paths & Tags Documentation

## Overview

This document lists all Docker images for the Leave Management System (LMS) microservices, their Docker Hub repository paths, available tags, and versioning strategy.

---

## Docker Hub Organization

**Docker Hub Organization**: `lms-microservices` (or your organization name)

**Base Registry URL**: `docker.io/lms-microservices/`

---

## Image Naming Convention

### Format
```
docker.io/{organization}/{service-name}:{tag}
```

### Tag Strategy
- `latest` - Latest stable production build
- `{version}` - Specific version (e.g., `1.0.0`, `2.1.5`)
- `{version}-build-{buildNumber}` - Build-specific tag
- `development` - Latest development build
- `staging` - Latest staging build
- `prod` - Latest production build

---

## Microservices Images

### 1. API Gateway Service

**Service Name**: `lms-api-gateway`

**Image Path**: `docker.io/lms-microservices/lms-api-gateway`

**Available Tags**:
```
docker.io/lms-microservices/lms-api-gateway:latest
```

**Dockerfile Location**: `src/ApiGateway/Dockerfile`

**Image Details**:
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Runtime**: ASP.NET Core 8.0
- **Port**: 5000:8080
- **Size**: ~327.97 MB

**Docker Compose Service Name**: `api-gateway`

---

### 2. Identity Service

**Service Name**: `lms-identity-service`

**Image Path**: `docker.io/lms-microservices/lms-identity-service`

**Available Tags**:
```
docker.io/lms-microservices/lms-identity-service:latest
```

**Dockerfile Location**: `src/Services/IdentityService/Dockerfile`

**Image Details**:
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Runtime**: ASP.NET Core 8.0
- **Port**: 5001:8080
- **Size**: ~339.53 MB
- **Key Responsibilities**: Authentication, JWT, User Management

**Docker Compose Service Name**: `identity-service`

---

### 3. Employee Service

**Service Name**: `lms-employee-service`

**Image Path**: `docker.io/lms-microservices/lms-employee-service`

**Available Tags**:
```
docker.io/lms-microservices/lms-employee-service:latest
```

**Dockerfile Location**: `src/Services/EmployeeService/Dockerfile`

**Image Details**:
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Runtime**: ASP.NET Core 8.0
- **Port**: 5002:8080
- **Size**: ~339.52 MB
- **Key Responsibilities**: Employee Data, Department Management, Reporting Structure

**Docker Compose Service Name**: `employee-service`

---

### 4. Leave Service

**Service Name**: `lms-leave-service`

**Image Path**: `docker.io/lms-microservices/lms-leave-service`

**Available Tags**:
```
docker.io/lms-microservices/lms-leave-service:latest
```

**Dockerfile Location**: `src/Services/LeaveService/Dockerfile`

**Image Details**:
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Runtime**: ASP.NET Core 8.0
- **Port**: 5003:8080
- **Size**: ~339.56 MB
- **Key Responsibilities**: Leave Management, Approvals, Leave Balance

**Docker Compose Service Name**: `leave-service`

---

### 5. Notification Service

**Service Name**: `lms-notification-service`

**Image Path**: `docker.io/lms-microservices/lms-notification-service`

**Available Tags**:
```
docker.io/lms-microservices/lms-notification-service:latest
```

**Dockerfile Location**: `src/Services/NotificationService/Dockerfile`

**Image Details**:
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Runtime**: ASP.NET Core 8.0
- **Port**: 5004:8080
- **Size**: ~339.48 MB
- **Key Responsibilities**: Email Notifications, Message Queue Consumption

**Docker Compose Service Name**: `notification-service`

---

## Infrastructure Images

### 6. PostgreSQL Databases

**Image Path**: `postgres:16-alpine`

**Compose Services**:
- `leave-db` - Leave Service Database
- `notification-db` - Notification Service Database  
- `employee-db` - Employee Service Database
- `identity-db` - Identity Service Database

**Configuration**:
- **Ports**: 5432 (default PostgreSQL port)
- **Version**: 16 (Alpine)
- **Environment Variables**:
  - `POSTGRES_USER`: postgres (from .env)
  - `POSTGRES_PASSWORD`: Admin password (from .env)
- **Volumes**: Persistent data storage

---

### 7. RabbitMQ (Message Broker)

**Image Path**: `rabbitmq:3.13-management-alpine`

**Compose Service**: `rabbitmq`

**Configuration**:
- **Ports**: 
  - 5672 (AMQP)
  - 15672 (Management UI)
- **Default User**: guest:guest
- **Management UI**: http://localhost:15672

---

### 8. Service Registry (Eureka)

**Image Path**: `steeltoe/netcorerun:net8` (or custom built Eureka service)

**Compose Service**: `eureka`

**Configuration**:
- **Port**: 8761
- **Eureka Dashboard**: http://localhost:8761

---

### 9. Jaeger (Distributed Tracing)

**Image Path**: `jaegertracing/all-in-one:latest`

**Compose Service**: `jaeger`

**Configuration**:
- **Ports**:
  - 5775/udp (Zipkin compact)
  - 6831/udp (Jaeger compact)
  - 6832/udp (Jaeger binary)
  - 16686 (UI)
- **Jaeger UI**: http://localhost:16686

---

## Image Build Commands

### Build All Services

```bash
# Build API Gateway
docker build -t lms-microservices/apigateway:1.1.0 \
  -f src/ApiGateway/Dockerfile .

# Build Identity Service
docker build -t lms-microservices/identityservice:1.1.0 \
  -f src/Services/IdentityService/Dockerfile .

# Build Employee Service
docker build -t lms-microservices/employeeservice:1.1.0 \
  -f src/Services/EmployeeService/Dockerfile .

# Build Leave Service
docker build -t lms-microservices/leaveservice:1.1.0 \
  -f src/Services/LeaveService/Dockerfile .

# Build Notification Service
docker build -t lms-microservices/notificationservice:1.1.0 \
  -f src/Services/NotificationService/Dockerfile .
```

### Build with Multiple Tags

```bash
docker build -t lms-microservices/apigateway:1.1.0 \
---

## Eureka Service Registry

**Image Path**: `steeltoeoss/eureka-server:latest`

**Compose Service**: `eureka-server`

**Configuration**:
- **Port**: 8761
- **URL**: http://localhost:8761
- **Dashboard**: Available at service URL

---

## Message Broker (RabbitMQ)

**Image Path**: `rabbitmq:3.13-management-alpine`

**Compose Service**: `rabbitmq`

**Configuration**:
- **Ports**: 
  - 5672 (AMQP)
  - 15672 (Management UI)
- **Default Credentials**: guest:guest
- **Management URL**: http://localhost:15672

---

## Distributed Tracing (Jaeger)

**Image Path**: `jaegertracing/all-in-one:latest`

**Compose Service**: `jaeger`

**Configuration**:
- **Ports**:
  - 16686 (UI)
  - 6831 (Jaeger agent - UDP)
  - 5775, 6832 (Other protocols)
- **Jaeger UI**: http://localhost:16686

---

## Docker Compose Configuration

### Service Reference

```yaml
version: '3.8'

services:
  # API Gateway
  api-gateway:
    image: lms-microservices/lms-api-gateway:latest
    container_name: api-gateway
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    networks:
      - lms-network

  # Identity Service
  identity-service:
    image: lms-microservices/lms-identity-service:latest
    container_name: identity-service
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=identity-db;Port=5432;Database=identity_db;User Id=postgres;Password=yourpassword;
    depends_on:
      - identity-db
    networks:
      - lms-network

  # Employee Service
  employee-service:
    image: lms-microservices/lms-employee-service:latest
    container_name: employee-service
    ports:
      - "5002:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=employee-db;Port=5432;Database=employee_db;User Id=postgres;Password=yourpassword;
    depends_on:
      - employee-db
    networks:
      - lms-network

  # Leave Service
  leave-service:
    image: lms-microservices/lms-leave-service:latest
    container_name: leave-service
    ports:
      - "5003:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=leave-db;Port=5432;Database=leave_db;User Id=postgres;Password=yourpassword;
    depends_on:
      - leave-db
      - employee-service
    networks:
      - lms-network

  # Notification Service
  notification-service:
    image: lms-microservices/lms-notification-service:latest
    container_name: notification-service
    ports:
      - "5004:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=notification-db;Port=5432;Database=notification_db;User Id=postgres;Password=yourpassword;
    depends_on:
      - notification-db
      - rabbitmq
    networks:
      - lms-network

  # Databases
  identity-db:
    image: postgres:16-alpine
    container_name: identity-db
    environment:
      - POSTGRES_DB=identity_db
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=yourpassword
    volumes:
      - identity_db_data:/var/lib/postgresql/data
    networks:
      - lms-network

  employee-db:
    image: postgres:16-alpine
    container_name: employee-db
    environment:
      - POSTGRES_DB=employee_db
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=yourpassword
    volumes:
      - employee_db_data:/var/lib/postgresql/data
    networks:
      - lms-network

  leave-db:
    image: postgres:16-alpine
    container_name: leave-db
    environment:
      - POSTGRES_DB=leave_db
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=yourpassword
    volumes:
      - leave_db_data:/var/lib/postgresql/data
    networks:
      - lms-network

  notification-db:
    image: postgres:16-alpine
    container_name: notification-db
    environment:
      - POSTGRES_DB=notification_db
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=yourpassword
    volumes:
      - notification_db_data:/var/lib/postgresql/data
    networks:
      - lms-network

  # Infrastructure Services
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    networks:
      - lms-network

  eureka-server:
    image: steeltoeoss/eureka-server:latest
    container_name: eureka-server
    ports:
      - "8761:8761"
    networks:
      - lms-network

  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: jaeger
    ports:
      - "16686:16686"
      - "6831:6831/udp"
    networks:
      - lms-network

volumes:
  identity_db_data:
  employee_db_data:
  leave_db_data:
  notification_db_data:

networks:
  lms-network:
    driver: bridge
```

---

## Quick Start Commands

### Build All Images

```bash
# Build locally
docker-compose build

# Or build individually
docker build -t lms-microservices/lms-api-gateway:latest -f src/ApiGateway/Dockerfile .
docker build -t lms-microservices/lms-identity-service:latest -f src/Services/IdentityService/Dockerfile .
docker build -t lms-microservices/lms-employee-service:latest -f src/Services/EmployeeService/Dockerfile .
docker build -t lms-microservices/lms-leave-service:latest -f src/Services/LeaveService/Dockerfile .
docker build -t lms-microservices/lms-notification-service:latest -f src/Services/NotificationService/Dockerfile .
```

### Run Services

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Health Checks

```bash
# API Gateway
curl http://localhost:5000/health

# Identity Service
curl http://localhost:5001/health

# Employee Service
curl http://localhost:5002/health

# Leave Service
curl http://localhost:5003/health

# Notification Service
curl http://localhost:5004/health

# Eureka Dashboard
open http://localhost:8761

# RabbitMQ Management
open http://localhost:15672

# Jaeger UI
open http://localhost:16686
```

---

**Document Version**: 2.0  
**Last Updated**: June 3, 2026  
**Status**: Current Production Setup

```bash
docker history lms-microservices/leaveservice:1.1.0
```

### Check Image Size

```bash
docker images lms-microservices/apigateway

# Output:
# REPOSITORY                           TAG       IMAGE ID      CREATED       SIZE
# lms-microservices/apigateway         latest    abc123def456  2 days ago    215MB
```

---

## Version History

### v1.1.0
- **Release Date**: June 1, 2026
- **Changes**:
  - Enhanced error handling
  - Improved logging
  - Performance optimizations
  - Added correlation ID support

### v1.0.1
- **Release Date**: May 28, 2026
- **Changes**:
  - Bug fixes in authentication flow
  - Database connection pooling improvement

### v1.0.0
- **Release Date**: May 15, 2026
- **Changes**:
  - Initial release
  - All core services functional

---

## Image Repository Information

### Organization: lms-microservices

**Docker Hub URL**: https://hub.docker.com/u/lms-microservices

**Visibility**: Public/Private (configure as needed)

**Description**: Leave Management System (LMS) Microservices

**Collaborators**:
- DevOps Team
- Architecture Team
- Development Team

---

## Container Runtime Configuration

### Memory and CPU Limits

```yaml
# Docker Compose Resource Limits
services:
  apigateway:
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
```

### Volume Mounts

```yaml
volumes:
  sqlserver-data:
    driver: local
  rabbitmq-data:
    driver: local

services:
  sqlserver:
    volumes:
      - sqlserver-data:/var/opt/mssql
```

---

## Security Considerations

### Image Scanning

```bash
# Scan image for vulnerabilities
docker scan lms-microservices/apigateway:latest

# Using Trivy
trivy image lms-microservices/apigateway:latest
```

### Private Registry

For private Docker registries, use:
```
docker.io/your-private-registry/lms-microservices/apigateway:1.1.0
```

### Image Signing

Implement Docker Content Trust (DCT):
```bash
export DOCKER_CONTENT_TRUST=1
docker push lms-microservices/apigateway:1.1.0
```

---

## Troubleshooting

### Image Not Found

```bash
# Ensure image is pulled
docker pull lms-microservices/apigateway:latest

# Check if image exists locally
docker images lms-microservices/apigateway
```

### Authentication Errors

```bash
# Login to Docker Hub
docker login
# Enter username and password

# Verify login
docker info
```

### Image Pull Timeout

```bash
# Increase timeout and retry
docker pull lms-microservices/leaveservice:latest --timeout=120s
```

### Container Exits Immediately

```bash
# Check container logs
docker logs <container-id>

# Run with verbose logging
docker run -e ASPNETCORE_ENVIRONMENT=Development \
           lms-microservices/apigateway:latest
```

---

## Best Practices

1. **Always use specific version tags** in production, not `latest`
2. **Scan images for vulnerabilities** before deployment
3. **Implement image retention policies** on Docker Hub
4. **Use multi-stage builds** to minimize image size
5. **Keep base images up to date**
6. **Document environment variables** for each service
7. **Use private registry** for sensitive applications
8. **Implement CI/CD for automated builds and pushes**

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Push Images

on:
  push:
    branches: [main]
    tags: ['v*']

jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: [apigateway, identityservice, employeeservice, 
                  leaveservice, notificationservice]
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Login to Docker Hub
        uses: docker/login-action@v2
        with:

