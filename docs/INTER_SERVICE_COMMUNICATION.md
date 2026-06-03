# Inter-Service Communication Writeup

## Overview
This document describes the inter-service communication patterns, architecture, and assumptions for the Leave Management System (LMS) microservices architecture.

## System Architecture

### Services in the LMS
1. **API Gateway** - Entry point for all external requests
2. **Identity Service** - Authentication and authorization
3. **Employee Service** - Employee data management
4. **Leave Service** - Leave request and management
5. **Notification Service** - Notification delivery

## Communication Patterns

### 1. Synchronous Communication (HTTP/REST)

#### Pattern Description
- Services call each other directly via HTTP requests
- Used for immediate request-response scenarios
- Blocked until response is received

#### Use Cases
- **API Gateway → Services**: Route incoming requests to appropriate services
- **Leave Service → Employee Service**: Validate employee existence, fetch employee details
- **Leave Service → Identity Service**: Verify user permissions, validate tokens
- **Any Service → Any Service**: Real-time data validation or retrieval

#### Implementation
- Uses HTTP/REST endpoints
- Implements circuit breakers for resilience
- Includes retry logic with 
- Request timeout configuratioexponential backoffns in place

---

### 2. Asynchronous Communication (Message Queue)

#### Pattern Description
- Services communicate via a message broker (RabbitMQ)
- Fire-and-forget messaging pattern
- Decoupled service interactions

#### Use Cases
- **Leave Service → Notification Service**: Send email notifications when leave is approved/rejected
- **Leave Service → Employee Service**: Notify of leave status updates
- **Any Service → Any Service**: Non-critical async operations

#### Benefits
- Reduced service coupling
- Better resilience to temporary failures
- Load leveling across services
- Easy to add new subscribers without modifying producer

---

### 3. API Gateway Pattern

#### Pattern Description
- All external traffic flows through the API Gateway (Ocelot)
- Gateway handles routing, authentication, rate limiting
- Simplifies client integration

#### Configuration Files
- `ocelot.eureka.json` - Service discovery configuration
- `ocelot.static.json` - Static route definitions

---

## Key Assumptions

### 1. Service Discovery
- **Assumption**: Services are registered in Eureka service registry
- **Rationale**: Dynamic service instance discovery in containerized environment
- **Implementation**: Configured in shared Eureka settings
- **Impact**: Services can find each other even after restarts/redeployment

### 2. Authentication & Authorization
- **Assumption**: All inter-service communication uses JWT tokens
- **Assumption**: Identity Service is the single source of truth for authentication
- **Rationale**: Centralized security policy management
- **Implementation**: JwtAuthenticationExtension validates all requests
- **Impact**: Token validation happens at each service boundary

### 3. Message Queue Reliability
- **Assumption**: RabbitMQ is running and available
- **Assumption**: Messages are persisted and will be retried on failure
- **Rationale**: Ensures no message loss in async communication
- **Implementation**: RabbitMQ configuration in appsettings
- **Impact**: Temporary service outages won't cause message loss

### 4. Correlation IDs
- **Assumption**: All requests include a correlation ID for tracing
- **Rationale**: Enables request tracking across multiple services
- **Implementation**: CorrelationIdMiddleware adds/validates correlation ID
- **Impact**: Simplifies debugging and monitoring distributed transactions

### 5. Health Checks
- **Assumption**: Each service exposes health check endpoints
- **Implementation**: HealthCheckMiddleware
- **Impact**: API Gateway and orchestrators can monitor service health

### 6. Service Instance Tracking
- **Assumption**: Services maintain awareness of their instance information
- **Implementation**: ServiceInstanceMiddleware provides instance details
- **Impact**: Enables better logging and request routing decisions

### 7. Observability & Tracing
- **Assumption**: OpenTelemetry is configured for distributed tracing
- **Rationale**: Track request flow across services
- **Implementation**: OpenTelemetryExtension in Shared.Common
- **Impact**: Central monitoring and debugging capabilities

### 8. Graceful Degradation
- **Assumption**: Services continue functioning even if dependent services are temporarily unavailable
- **Rationale**: Increase overall system resilience
- **Implementation**: Circuit breakers, timeouts, fallback mechanisms
- **Impact**: Non-critical failures don't cascade through the system

### 9. Data Consistency
- **Assumption**: Strong consistency is required for critical operations
- **Rationale**: Leave approvals and employee records must be accurate
- **Implementation**: Synchronous calls for critical operations
- **Impact**: Latency trade-off for data accuracy

### 10. Network Communication
- **Assumption**: All services run in a Docker Compose environment
- **Assumption**: Services communicate via internal Docker network
- **Rationale**: Secure inter-service communication, simpler networking
- **Implementation**: Configured in docker-compose.yml
- **Impact**: Services can resolve each other by service name

---

## Communication Flow Examples

### Example 1: Leave Request Submission
```
Client → API Gateway → Leave Service → [Identity Service (validate)] 
       → [Employee Service (check employee)] → Database
       → RabbitMQ → Notification Service → Send notification
```

### Example 2: Get Leave Details
```
Client → API Gateway → Leave Service → [Identity Service (validate)] 
       → [Employee Service (get details)] 
       → Return response to client
```

### Example 3: Employee Status Update
```
Employee Service → RabbitMQ → Leave Service (listen)
                           → Notification Service (listen)
```

---

## Error Handling Assumptions

### Service Unavailable
- **Assumption**: Services implement circuit breakers
- **Fallback**: Return cached data or friendly error message
- **Retry**: Implement exponential backoff for transient failures

### Invalid Requests
- **Assumption**: Each service validates incoming requests
- **Response**: Return 400 Bad Request with error details
- **Logging**: Log validation failures for debugging

### Authentication Failures
- **Assumption**: Invalid/expired tokens are rejected
- **Response**: Return 401 Unauthorized
- **Action**: Client must re-authenticate

---

## Deployment Considerations

### Service Dependencies
- Database migrations must complete before service startup
- Eureka server must be running before services register
- RabbitMQ must be accessible for async communication

### Scaling
- Services can be scaled independently
- Load balancer distributes traffic to API Gateway instances
- Message queue ensures load distribution for async tasks

### Monitoring
- Each service exposes metrics via OpenTelemetry
- Correlation IDs enable end-to-end tracing
- Health checks indicate service readiness

---

## Security Assumptions

1. **Internal Communication**: Assumed to be on secure Docker network
2. **Token Exchange**: JWT tokens validated at each service boundary
3. **API Gateway**: Enforces authentication before routing requests

---
