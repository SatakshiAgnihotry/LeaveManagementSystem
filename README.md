# Leave Management System (LMS) - Microservices Architecture

Welcome to the Leave Management System microservices project! This is a comprehensive, production-ready microservices architecture built with .NET 8.0, featuring authentication, employee management, leave processing, and notifications.

# Video Recording Link:
https://nagarro-my.sharepoint.com/:v:/r/personal/satakshi_agnihotri_nagarro_com/Documents/Recordings/Meeting%20with%20Satakshi%20Agnihotri-20260604_130419-Meeting%20Recording.mp4?csf=1&web=1&e=P3ad16&nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJTdHJlYW1XZWJBcHAiLCJyZWZlcnJhbFZpZXciOiJTaGFyZURpYWxvZy1MaW5rIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXcifX0%3D

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Project Setup](#project-setup)
- [Environment Variables](#environment-variables)
- [Running with Docker Compose](#running-with-docker-compose)
- [API Testing Instructions](#api-testing-instructions)
- [Project Structure](#project-structure)
- [Troubleshooting](#troubleshooting)
- [Resources & Documentation](#resources--documentation)
- [Contributing](#contributing)
- [Support](#support)

---

## 🎯 Project Overview

The Leave Management System is a cloud-native microservices application designed to manage employee leave requests, approvals, and notifications across an organization.

### Key Features

✅ **Microservices Architecture** - Independent, scalable services  
✅ **JWT Authentication** - Secure token-based authentication  
✅ **Leave Management** - Request, approve, and track leave  
✅ **Employee Directory** - Centralized employee database  
✅ **Email Notifications** - Automated notifications via RabbitMQ  
✅ **Distributed Tracing** - Observe system behavior with Jaeger  
✅ **Service Discovery** - Dynamic service registration with Eureka  
✅ **Docker Containerization** - Full containerized deployment  

---

## 🏗️ Architecture

The system consists of 5 core microservices:

```
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway (5000)                      │
│                    (Ocelot - Routing)                        │
└──────────────┬──────────────┬──────────────┬─────────────────┘
               │              │              │
      ┌────────▼──┐    ┌─────▼──┐    ┌──────▼────┐
      │  Identity  │    │Employee │    │   Leave   │
      │ Service    │    │ Service │    │ Service   │
      │  (5001)    │    │ (5002)  │    │  (5003)   │
      └────────┬───┘    └────┬────┘    └───┬───────┘
               │             │             │
               └─────────┬───┴─────────────┘
                         │
                  ┌──────▼──────┐
                  │ RabbitMQ    │
                  │  (5672)     │
                  └──────┬──────┘
                         │
                  ┌──────▼──────────────┐
                  │ Notification        │
                  │ Service (5004)      │
                  └─────────────────────┘
```

### Microservices

| Service | Port | Purpose |
|---------|------|---------|
| **API Gateway** | 5000 | Entry point, routing, authentication |
| **Identity Service** | 5001 | User authentication, JWT tokens |
| **Employee Service** | 5002 | Employee data management |
| **Leave Service** | 5003 | Leave request processing |
| **Notification Service** | 5004 | Email notifications |

### Infrastructure Services

| Service | Port | Purpose |
|---------|------|---------|
| **SQL Server** | 1433 | Database (per-service pattern) |
| **RabbitMQ** | 5672 / 15672 | Message broker |
| **Eureka** | 8761 | Service registry |
| **Jaeger** | 16686 | Distributed tracing |

---

## ✅ Prerequisites

Before you begin, ensure you have the following installed:

### Required Software

- **Docker Desktop** (v24.0+) - [Download](https://www.docker.com/products/docker-desktop)
- **Docker Compose** (v2.0+) - Included with Docker Desktop
- **.NET SDK 8.0** (optional, for local development) - [Download](https://dotnet.microsoft.com/download)
- **Git** (for cloning repository) - [Download](https://git-scm.com/)
- **Postman** (for API testing) - [Download](https://www.postman.com/downloads/)
- **VS Code** (optional, recommended) - [Download](https://code.visualstudio.com/)

### System Requirements

- **OS**: Windows 10/11, macOS 10.14+, or Linux
- **RAM**: Minimum 8GB (16GB recommended)
- **Disk Space**: 20GB free space
- **CPU**: Multi-core processor recommended

### Verify Installation

```bash
# Check Docker
docker --version
# Output: Docker version 24.0.0, build abcdef

# Check Docker Compose
docker-compose --version
# Output: Docker Compose version v2.0.0

# Check .NET (optional)
dotnet --version
# Output: 8.0.0
```

---

## 🚀 Project Setup

### Step 1: Clone the Repository

```bash
# Clone the LMS repository
git clone https://github.com/your-org/lms-microservices.git

# Navigate to project directory
cd LMS
```

### Step 2: Verify Project Structure

```bash
# Windows
dir

# macOS/Linux
ls -la
```

Expected output:
```
docker-compose.yml
LMS.sln
postman/
src/
```

### Step 3: Create Environment File

```bash
# Create .env file in project root
# Windows (PowerShell)
New-Item -ItemType File -Name ".env"

# macOS/Linux
touch .env
```

### Step 4: Configure Environment Variables

Add the following to `.env` file:

```env
# Database Configuration
SA_PASSWORD=YourStrong@Pwd123!
DB_SERVER=postgreSQL
DB_PORT=1433
DB_USER=sa

# JWT Configuration
JWT_SECRET=your-super-secret-jwt-key-min-32-characters-long!
JWT_EXPIRY=86400

# RabbitMQ Configuration
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672

# Service URLs
IDENTITY_SERVICE_URL=http://identityservice:5000
EMPLOYEE_SERVICE_URL=http://employeeservice:5000
LEAVE_SERVICE_URL=http://leaveservice:5000

# Eureka Configuration
EUREKA_SERVER_URL=http://eureka:8761
```

### Step 5: Verify File Structure

```
LMS/
├── .env                          # Environment variables
├── .dockerignore
├── docker-compose.yml            # Docker Compose configuration
├── LMS.sln                        # Visual Studio Solution
├── postman/
│   └── LeaveManagementSystem.json # Postman collection
└── src/
    ├── ApiGateway/
    │   ├── Dockerfile
    │   ├── ApiGateway.csproj
    │   └── ...
    ├── Services/
    │   ├── IdentityService/
    │   ├── EmployeeService/
    │   ├── LeaveService/
    │   └── NotificationService/
    └── Shared.Common/
```

---

## 🔧 Environment Variables

### Complete Environment Configuration

```env
# ============================================
# DATABASE CONFIGURATION
# ============================================
SA_PASSWORD=YourStrong@Pwd123!
DB_SERVER=postgresql
DB_PORT=1433
DB_USER=sa

# ============================================
# JWT AUTHENTICATION
# ============================================
JWT_SECRET=your-super-secret-jwt-key-must-be-at-least-32-chars!
JWT_EXPIRY=86400
JWT_ISSUER=lms-microservices
JWT_AUDIENCE=lms-api-clients

# ============================================
# RABBITMQ MESSAGE BROKER
# ============================================
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_VHOST=/

# ============================================
# SERVICE DISCOVERY (EUREKA)
# ============================================
EUREKA_SERVER_URL=http://eureka:8761/eureka
EUREKA_INSTANCE_HOSTNAME=localhost
EUREKA_CLIENT_SERVICEURL_DEFAULTZONE=http://eureka:8761/eureka/

# ============================================
# SERVICE URLS
# ============================================
IDENTITY_SERVICE_URL=http://identityservice:5000
EMPLOYEE_SERVICE_URL=http://employeeservice:5000
LEAVE_SERVICE_URL=http://leaveservice:5000
API_GATEWAY_URL=http://localhost:5000

# ============================================
# LOGGING & MONITORING
# ============================================
LOG_LEVEL=Information
JAEGER_AGENT_HOST=jaeger
JAEGER_AGENT_PORT=6831
OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317

# ============================================
# APPLICATION SETTINGS
# ============================================
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
```

### Environment Variables by Service

#### API Gateway
```env
OCELOT_CONFIG_PATH=ocelot.eureka.json
EUREKA_SERVER_URL=http://eureka:8761/eureka
```

#### Identity Service
```env
ConnectionStrings__DefaultConnection=Server=postgreSQL;Database=IdentityDB;User Id=sa;Password=${SA_PASSWORD};Encrypt=false;
JWT_SECRET=${JWT_SECRET}
JWT_EXPIRY=${JWT_EXPIRY}
```

#### Employee Service
```env
ConnectionStrings__DefaultConnection=Server=postgreSQL;Database=EmployeeDB;User Id=sa;Password=${SA_PASSWORD};Encrypt=false;
RABBITMQ_CONNECTION_STRING=amqp://${RABBITMQ_USER}:${RABBITMQ_PASSWORD}@${RABBITMQ_HOST}:${RABBITMQ_PORT}/
```

#### Leave Service
```env
ConnectionStrings__DefaultConnection=Server=postgreSQL;Database=LeaveDB;User Id=sa;Password=${SA_PASSWORD};Encrypt=false;
RABBITMQ_CONNECTION_STRING=amqp://${RABBITMQ_USER}:${RABBITMQ_PASSWORD}@${RABBITMQ_HOST}:${RABBITMQ_PORT}/
EMPLOYEE_SERVICE_URL=${EMPLOYEE_SERVICE_URL}
IDENTITY_SERVICE_URL=${IDENTITY_SERVICE_URL}
```

#### Notification Service
```env
RABBITMQ_CONNECTION_STRING=amqp://${RABBITMQ_USER}:${RABBITMQ_PASSWORD}@${RABBITMQ_HOST}:${RABBITMQ_PORT}/
SMTP_SERVER=${SMTP_SERVER}
SMTP_PORT=${SMTP_PORT}
SMTP_USER=${SMTP_USER}
SMTP_PASSWORD=${SMTP_PASSWORD}
```

---

## 🐳 Running with Docker Compose

### Step 1: Start All Services

```bash
# Navigate to project root
cd LMS

# Start all services in detached mode
docker-compose up -d

# View startup progress
docker-compose logs -f
```

### Step 2: Verify Services Are Running

```bash
# Check all containers
docker-compose ps

# Expected output:
# NAME                    COMMAND                  SERVICE             STATUS
# lms-apigateway-1        "/bin/sh -c 'dotnet …"  apigateway          Up 2 minutes
# lms-identityservice-1   "/bin/sh -c 'dotnet …"  identityservice     Up 2 minutes
# lms-employeeservice-1   "/bin/sh -c 'dotnet …"  employeeservice     Up 2 minutes
# lms-leaveservice-1      "/bin/sh -c 'dotnet …"  leaveservice        Up 2 minutes
# lms-notificationservice-1 "/bin/sh -c 'dotnet …" notificationservice Up 2 minutes
# lms-postgreSQL-1         "/bin/sh -c 'sqlservr …" postgreSQL          Up 2 minutes
# lms-rabbitmq-1          "docker-entrypoint.s…"  rabbitmq            Up 2 minutes
# lms-eureka-1            "/bin/sh -c 'dotnet …"  eureka              Up 2 minutes
# lms-jaeger-1            "/go/bin/all-in-one-…"  jaeger              Up 2 minutes
```

### Step 3: Check Service Health

```bash
# API Gateway Health
curl http://localhost:5000/health

# Identity Service Health
curl http://localhost:5001/health

# View logs for specific service
docker-compose logs leaveservice

# Follow logs in real-time
docker-compose logs -f apigateway
```

### Step 4: Access Service UIs

Open in your browser:

- **API Gateway**: http://localhost:5000
- **Eureka Dashboard**: http://localhost:8761
- **RabbitMQ Management**: http://localhost:15672 (guest:guest)
- **Jaeger UI**: http://localhost:16686

### Common Docker Compose Commands

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# View running containers
docker-compose ps

# View logs
docker-compose logs [service-name]

# Tail logs
docker-compose logs -f [service-name]

# Execute command in container
docker-compose exec [service-name] [command]

# Rebuild images
docker-compose build

# Rebuild and restart specific service
docker-compose up -d --build [service-name]

# Scale a service
docker-compose up -d --scale employeeservice=3
```

---

## 🧪 API Testing Instructions

### Option 1: Using Postman (Recommended)

#### Step 1: Import Postman Collection

1. Open **Postman**
2. Click **Import** button (top-left)
3. Select **File** tab
4. Navigate to `postman/LeaveManagementSystem.json`
5. Click **Import**

#### Step 2: Set Environment Variables

1. Click **Environments** (left sidebar)
2. Click **+** to create new environment
3. Add the following variables:
   ```
   base_url: http://localhost:5000
   token: (will be filled after login)
   correlation_id: 550e8400-e29b-41d4-a716-446655440000
   ```
4. Save environment as "Local Development"

#### Step 3: Run API Tests

```
Authentication
├── Login
├── Verify Token
├── Refresh Token

Employee Management
├── List Employees
├── Get Leave Balance
├── Create Employee

Leave Management
├── Submit Leave Request
├── Get Leave Details
├── Approve Leave
├── Reject Leave
└── Cancel Leave

Notifications
├── Get Notifications
├── Mark as Read
```

### Option 2: Using cURL

#### Login and Get Token

```bash
# Login
TOKEN_RESPONSE=$(curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@company.com",
    "password": "Admin@123"
  }')

# Extract token (Unix/macOS)
TOKEN=$(echo $TOKEN_RESPONSE | jq -r '.data.token')

# Extract token (Windows PowerShell)
$TOKEN = ($TOKEN_RESPONSE | ConvertFrom-Json).data.token
```

#### Get Employees

```bash
curl -X GET http://localhost:5000/api/employees \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000"
```

#### Submit Leave Request

```bash
curl -X POST http://localhost:5000/api/leaves/apply \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{
    "leaveTypeId": "lt-001",
    "startDate": "2024-06-15",
    "endDate": "2024-06-17",
    "appliedFor": "Full Day",
    "reason": "Family vacation"
  }'
```


## 📁 Project Structure

```
LMS/
├── docker-compose.yml              # Orchestration configuration
├── LMS.sln                          # Visual Studio solution
├── README.md                        # This file
├── .env                             # Environment variables
├── .dockerignore                    # Docker ignore patterns
│
├── src/
│   ├── ApiGateway/                 # API Gateway Service
│   │   ├── Dockerfile
│   │   ├── Program.cs
|   |   ├── Extension/
│   │   ├── ocelot.static.json
│   │   ├── ocelot.eureka.json
│   │   └── ApiGateway.csproj
│   │
│   ├── Services/
│   │   ├── IdentityService/        # Authentication Service
│   │   │   ├── Controllers/
│   │   │   ├── Services/
│   │   │   ├── Models/
|   |   |   ├── Extension/
│   │   │   ├── Data/
│   │   │   ├── Dockerfile
│   │   │   └── Program.cs
│   │   │
│   │   ├── EmployeeService/        # Employee Management
│   │   │   ├── Controllers/
│   │   │   ├── Services/
│   │   │   ├── Models/
|   |   |   ├── Extension/
│   │   │   ├── Migrations/
│   │   │   ├── Dockerfile
│   │   │   └── Program.cs
│   │   │
│   │   ├── LeaveService/           # Leave Management
│   │   │   ├── Controllers/
│   │   │   ├── Services/
│   │   │   ├── Models/
|   |   |   ├── Extension/
│   │   │   ├── Migrations/
│   │   │   ├── Dockerfile
│   │   │   └── Program.cs
│   │   │
│   │   └── NotificationService/    # Notifications
│   │       ├── Services/
│   │       ├── Models/
|   |       ├── Extension/
│   │       ├── Dockerfile
│   │       └── Program.cs
│   │
│   └── Shared.Common/              # Shared Libraries
│       ├── Configuration/
│       ├── Extensions/
│       ├── Middleware/
│       └── Shared.Common.csproj
│
└── postman/
    └── LeaveManagementSystem.json  # Postman collection
```

---

## 🔧 Troubleshooting

### Issue: Containers fail to start

```bash
# Check logs
docker-compose logs

# Rebuild and restart
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

### Issue: Database connection error

```bash
# Verify SQL Server is running
docker-compose ps postgreSQL

# Check database logs
docker-compose logs postgreSQL

# Try restarting SQL Server
docker-compose restart postgreSQL
```

### Issue: Port already in use

```bash
# Find process using port 5000
# Windows
netstat -ano | findstr :5000

# macOS/Linux
lsof -i :5000

# Kill process and restart Docker
docker-compose down
docker-compose up -d
```

### Issue: RabbitMQ connection errors

```bash
# Verify RabbitMQ is running
docker-compose ps rabbitmq

# Check RabbitMQ logs
docker-compose logs rabbitmq

# Restart RabbitMQ
docker-compose restart rabbitmq
```

### Issue: Eureka not registering services

```bash
# Check Eureka logs
docker-compose logs eureka

# Verify service registration on dashboard
# Open http://localhost:8761 in browser
```

### Issue: Cannot login - invalid credentials

```bash
# Default test credentials:
Email: admin@company.com
Password: Admin@123

# Or create new user via Employee Service
# Then register in Identity Service
```

### Viewing Real-time Logs

```bash
# Watch all services
docker-compose logs -f

# Watch specific service
docker-compose logs -f leaveservice

# Show last 100 lines
docker-compose logs --tail=100

# Follow specific service with timestamps
docker-compose logs -f --timestamps identityservice
```

---

## 📚 Resources & Documentation

### Documentation Files

- [MICROSERVICES_DESIGN.md](MICROSERVICES_DESIGN.md) - Detailed architecture
- [API_ENDPOINTS_DOCUMENTATION.md](API_ENDPOINTS_DOCUMENTATION.md) - Complete API reference
- [INTER_SERVICE_COMMUNICATION.md](INTER_SERVICE_COMMUNICATION.md) - Communication patterns
- [DOCKER_HUB_IMAGES.md](DOCKER_HUB_IMAGES.md) - Container images reference

### External Resources

- **.NET 8.0 Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **Ocelot API Gateway**: https://ocelot.readthedocs.io/
- **Docker Documentation**: https://docs.docker.com/
- **RabbitMQ**: https://www.rabbitmq.com/documentation.html
- **Eureka Service Discovery**: https://github.com/Netflix/eureka/wiki
- **Jaeger Tracing**: https://www.jaegertracing.io/docs/

### Video Tutorials

📹 **Project Setup & Architecture Overview**
- [YouTube Link](https://www.youtube.com/watch?v=example-video-id-1)
- Duration: 15 minutes
- Topics: Setup, architecture explanation, service overview

📹 **Running with Docker Compose**
- [YouTube Link](https://www.youtube.com/watch?v=example-video-id-2)
- Duration: 12 minutes
- Topics: Docker setup, running services, health checks

📹 **API Testing with Postman**
- [YouTube Link](https://www.youtube.com/watch?v=example-video-id-3)
- Duration: 20 minutes
- Topics: Postman setup, authentication, testing workflows

📹 **Microservices Communication & Messaging**
- [YouTube Link](https://www.youtube.com/watch?v=example-video-id-4)
- Duration: 18 minutes
- Topics: Service-to-service calls, RabbitMQ, async patterns

📹 **Debugging & Monitoring**
- [YouTube Link](https://www.youtube.com/watch?v=example-video-id-5)
- Duration: 15 minutes
- Topics: Jaeger tracing, logging, troubleshooting

### Recommended Learning Path

1. **Start Here**: Read this README.md
2. **Architecture**: Review [MICROSERVICES_DESIGN.md](MICROSERVICES_DESIGN.md)
3. **Setup**: Follow setup instructions below
4. **API Testing**: Use Postman collection
5. **Deep Dive**: Read [API_ENDPOINTS_DOCUMENTATION.md](API_ENDPOINTS_DOCUMENTATION.md)

---

## 📝 Quick Start Checklist

- [ ] Install Docker Desktop
- [ ] Clone repository
- [ ] Create `.env` file
- [ ] Configure environment variables
- [ ] Run `docker-compose up -d`
- [ ] Verify services are running
- [ ] Import Postman collection
- [ ] Test login endpoint
- [ ] Create test leave request
- [ ] Check notifications

---

## 🤝 Contributing

### Development Workflow

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Changes**
   - Follow coding standards
   - Add unit tests
   - Update documentation

3. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat: description of feature"
   ```

4. **Push and Create PR**
   ```bash
   git push origin feature/your-feature-name
   ```

### Code Style Guidelines

- Follow Microsoft C# coding conventions
- Use meaningful variable names
- Add XML documentation comments
- Keep methods focused and testable

---


## 🎉 Next Steps

After successful setup:

1. **Explore APIs**: Use Postman to test all endpoints
2. **Read Documentation**: Check out detailed docs
3. **Understand Architecture**: Review microservices design
4. **Try Local Development**: Set up .NET environment for coding
5. **Watch Videos**: Catch up with video tutorials

---

## 📊 Project Statistics

| Metric | Value |
|--------|-------|
| Microservices | 5 |
| Infrastructure Services | 4 |
| Total Containers | 9 |
| API Endpoints | 10+ |
| Documentation Files | 4 |

---

## ✨ Features at a Glance

| Feature | Status | Details |
|---------|--------|---------|
| Leave Request Management | ✅ | Submit, track, approve/reject |
| Employee Management | ✅ | CRUD operations, hierarchy |
| Authentication | ✅ | JWT-based security |
| Notifications | ✅ | RabbitMQ integration |
| Service Discovery | ✅ | Eureka registry |
| Distributed Tracing | ✅ | Jaeger monitoring |
| API Documentation | ✅ | Postman collection |
| Docker Support | ✅ | Full containerization |

---

**Created**: May 30, 2026  
**Last Updated**: May 30, 2026  
**Version**: 1.0.0
