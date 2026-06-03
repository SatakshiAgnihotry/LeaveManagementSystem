# API Endpoints Documentation (Updated)

## Table of Contents
1. Authentication Service
2. Employee Service
3. Leave Service
4. Notification Service
5. Common Response Formats
6. Error Codes & Messages

---

## Authentication Service

### Base URL
```
http://localhost:5000/api/auth
```

### 1. User Login

**Endpoint**: `POST /auth/login`

**Description**: Authenticate user and receive JWT token for subsequent requests

**Authentication**: None (Public endpoint)

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": null,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": "usr-emp-001",
    "name": "Arav",
    "email": "memp@gmail.com",
    "role": "Emp",
    "managerId": "usr-mgr-001",
    "expiresAt": "2026-06-03T16:26:57.1772473Z"
  },
  "errors": null
}
```

**Error Response (401 Unauthorized)**:
```json
{
  "success": false,
  "message": "Invalid mail or password",
  "data": null,
  "errors": null
}
```


---

## Employee Service

### Base URL
```
http://localhost:5000/api/employees
```

### 1. Get Team Employees

**Endpoint**: `GET /employees/team`

**Description**: Retrieve list of employees on the user's team

**Authentication**: Required (Bearer Token)

**Request Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
X-Correlation-ID: {uuid}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "employeeId": "usr-emp-001",
      "name": "Arav",
      "email": "memp@gmail.com",
      "role": "Emp",
      "managerId": "usr-mgr-001",
      "createdAt": "2026-01-01T00:00:00Z"
    },
    {
      "employeeId": "usr-emp-002",
      "name": "Mini",
      "email": "emp2@gmail.com",
      "role": "Emp",
      "managerId": "usr-mgr-001",
      "createdAt": "2026-06-02T05:57:31.760462Z"
    }
  ],
  "errors": null
}
```

---

### 2. Get Leave Balances

**Endpoint**: `GET /employees/{employeeId}/leave-balances`

**Description**: Retrieve leave balance information for a specific employee

**Authentication**: Required (Bearer Token)

**Path Parameters**:
- `employeeId` (string, required): The unique identifier of the employee

**Request Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
X-Correlation-ID: {uuid}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": 4,
      "employeeId": "usr-emp-001",
      "leaveType": "Casual",
      "totalAllocated": 15,
      "used": 0,
      "remaining": 15
    },
    {
      "id": 5,
      "employeeId": "usr-emp-001",
      "leaveType": "Sick",
      "totalAllocated": 15,
      "used": 0,
      "remaining": 15
    },
    {
      "id": 6,
      "employeeId": "usr-emp-001",
      "leaveType": "Privilege",
      "totalAllocated": 15,
      "used": 0,
      "remaining": 15
    }
  ],
  "errors": null
}
```

**Error Response (404 Not Found)**:
```json
{
  "success": false,
  "message": "Employee not found.",
  "data": null,
  "errors": null
}
```

---

### 3. Create Employee

**Endpoint**: `POST /employees`

**Description**: Create a new employee record with auto-assigned leave balances

**Authentication**: Required (Bearer Token - Admin/HR role)

**Request Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
X-Correlation-ID: {uuid}
```

**Request Body**:
```json
{
  "name": "Employee Name",
  "email": "employee@company.com"
}
```

**Success Response (201 Created)**:
```json
{
  "success": true,
  "message": "Employee created successfully with auto-assigned leave balances.",
  "data": {
    "employeeId": "usr-emp-003",
    "name": "Harshit",
    "email": "emp3@gmail.com",
    "role": "Emp",
    "managerId": "usr-mgr-001",
    "createdAt": "2026-06-03T08:36:09.6180852Z"
  },
  "errors": null
}
```

**Error Response (409 Conflict)**:
```json
{
  "success": false,
  "message": "An employee with this email already exists.",
  "data": null,
  "errors": null
}
```


---

## Leave Service

### Base URL
```
http://localhost:5000/api/leaves
```

### 1. Apply Leave

**Endpoint**: `POST /leaves/apply`

**Description**: Submit a new leave request for approval by manager

**Authentication**: Required (Bearer Token)

**Request Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
X-Correlation-ID: {uuid}
```

**Request Body**:
```json
{
  "leaveType": "Sick",
  "startDate": "2026-07-11",
  "endDate": "2026-07-12",
  "reason": "Personal work"
}
```

**Success Response (201 Created)**:
```json
{
  "success": true,
  "message": "Leave request submitted successfully.",
  "data": {
    "leaveRequestId": "5c7a7e0f-69c4-424a-b9ed-c32e48ba17b8",
    "employeeId": "usr-emp-001",
    "employeeName": "Arav",
    "managerId": "usr-mgr-001",
    "leaveType": "Sick",
    "startDate": "2026-07-11T00:00:00Z",
    "endDate": "2026-07-12T00:00:00Z",
    "numberOfDays": 1,
    "reason": "Personal work",
    "status": "Pending",
    "comments": null,
    "createdAt": "2026-06-03T08:38:45.6060392Z",
    "updatedAt": null
  },
  "errors": null
}
```

**Error Response (409 Conflict)**:
```json
{
  "success": false,
  "message": "You already have a pending or approved leave in this date range.",
  "data": null,
  "errors": null
}
```

---

### 2. Approve Leave

**Endpoint**: `PUT /leaves/{leaveRequestId}/approve`

**Description**: Approve a pending leave request (Manager/Approver action)

**Authentication**: Required (Bearer Token - Approver role)

**Path Parameters**:
- `leaveRequestId` (string, required): The unique identifier of the leave request

**Request Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
X-Correlation-ID: {uuid}
```

**Request Body**:
```json
{
  "comments": "Approved"
}
```

**Error Response (400 Bad Request)**:
```json
{
  "Success": false,
  "Message": "Only Pending requests can be approved. Current status: Approved",
  "Data": null,
  "Errors": null
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Leave approved successfully.",
  "data": {
    "leaveRequestId": "5c7a7e0f-69c4-424a-b9ed-c32e48ba17b8",
    "employeeId": "usr-emp-001",
    "employeeName": "Arav",
    "status": "Approved",
    "approvalDate": "2026-06-03T09:00:00Z"
  },
  "errors": null
}
```

---

### 3. Reject Leave

**Endpoint**: `PUT /leaves/{leaveRequestId}/reject`

**Description**: Reject a pending leave request with optional comments

**Authentication**: Required (Bearer Token - Approver role)

**Path Parameters**:
- `leaveRequestId` (string, required): The unique identifier of the leave request

**Request Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
X-Correlation-ID: {uuid}
```

**Request Body**:
```json
{
  "comments": "Rejected due to workload"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Leave rejected.",
  "data": {
    "leaveRequestId": "00bae4b5-41c7-4469-90d0-e52d81f5a5aa",
    "employeeId": "usr-emp-001",
    "employeeName": "Arav",
    "managerId": "usr-mgr-001",
    "leaveType": "Sick",
    "startDate": "2026-07-10T00:00:00Z",
    "endDate": "2026-07-11T00:00:00Z",
    "numberOfDays": 1,
    "reason": "Personal work",
    "status": "Rejected",
    "comments": "Rejected due to workload",
    "createdAt": "2026-06-03T08:48:10.670998Z",
    "updatedAt": "2026-06-03T08:48:31.3092608Z"
  },
  "errors": null
}
```

---

### 4. Cancel Leave

**Endpoint**: `DELETE /leaves/{leaveRequestId}/cancel`

**Description**: Cancel an approved or pending leave request

**Authentication**: Required (Bearer Token)

**Path Parameters**:
- `leaveRequestId` (string, required): The unique identifier of the leave request

**Request Headers**:
```
Authorization: Bearer {token}
X-Correlation-ID: {uuid}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Leave cancelled successfully.",
  "data": {
    "leaveRequestId": "acc7b0cf-d0f2-4bf2-a4f6-af51aec8ebd9",
    "employeeId": "usr-emp-001",
    "employeeName": "Arav",
    "managerId": "usr-mgr-001",
    "leaveType": "Sick",
    "startDate": "2026-07-09T00:00:00Z",
    "endDate": "2026-07-10T00:00:00Z",
    "numberOfDays": 1,
    "reason": "Personal work",
    "status": "Cancelled",
    "comments": null,
    "createdAt": "2026-06-03T08:49:03.814386Z",
    "updatedAt": "2026-06-03T08:49:32.0125501Z"
  },
  "errors": null
}
```

---

### 5. Circuit Breaker Test

**Endpoint**: `GET /leaves/circuit-breaker/test/{input}`

**Description**: Test circuit breaker pattern with different inputs (success/error)

**Authentication**: Required (Bearer Token)

**Path Parameters**:
- `input` (string, required): Test input - use `success` for successful response or `error` for failure

**Request Headers**:
```
Authorization: Bearer {token}
X-Correlation-ID: {uuid}
```

**Success Response (200 OK)**:
```json
{
  "status": "SUCCESS",
  "instance": "f1b8bdfd0b76",
  "userServiceResponse": "SUCCESS: {\"message\":\"Success! Input was: success\",\"instance\":\"74e2bf4c9f2a\",\"timestamp\":\"2026-06-03T10:27:38.5929744Z\"}",
  "durationMs": 426,
  "input": "success"
}
```

**Error Response (503 Service Unavailable - Employee Service Error)**:
```json
{
  "status": "FAILED",
  "instance": "f1b8bdfd0b76",
  "error": "EmployeeService returned InternalServerError",
  "durationMs": 6735,
  "input": "error"
}
```

**Error Response (503 Service Unavailable - Circuit Open)**:
```json
{
  "status": "FAILED",
  "instance": "f1b8bdfd0b76",
  "error": "The circuit is now open and is not allowing calls.",
  "durationMs": 5,
  "input": "error"
}
```

---

## Notification Service

### Base URL
```
http://localhost:5000/api/notifications
```

### 1. Get Notifications

**Endpoint**: `GET /notifications`

**Description**: Retrieve list of notifications for the authenticated user

**Authentication**: Required (Bearer Token)

**Query Parameters**:
- `pageNumber` (integer, optional): Page number for pagination (default: 1)
- `pageSize` (integer, optional): Records per page (default: 10)
- `isRead` (boolean, optional): Filter by read status (true/false)

**Request Headers**:
```
Authorization: Bearer {token}
X-Correlation-ID: {uuid}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "notificationId": "9f75b53e-681c-47f6-8afb-b1b99543410b",
      "userId": "usr-emp-001",
      "message": "Your Sick leave (2026-07-11 to 2026-07-12) has been submitted for approval.",
      "type": "LeaveSubmitted",
      "isRead": false,
      "createdAt": "2026-06-03T08:38:45.6060392Z"
    },
    {
      "notificationId": "notif-002",
      "userId": "usr-emp-001",
      "message": "Your leave request has been approved.",
      "type": "LeaveApproved",
      "isRead": true,
      "createdAt": "2026-06-02T10:00:00Z"
    }
  ],
  "errors": null
}
```

---

### 2. Mark Notification as Read

**Endpoint**: `PUT /notifications/{notificationId}/read`

**Description**: Mark a specific notification as read

**Authentication**: Required (Bearer Token)

**Path Parameters**:
- `notificationId` (string, required): The unique identifier of the notification

**Request Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
X-Correlation-ID: {uuid}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Notification marked as read",
  "data": {
    "notificationId": "9f75b53e-681c-47f6-8afb-b1b99543410b",
    "isRead": true,
    "readAt": "2026-06-03T09:00:00Z"
  },
  "errors": null
}
```

---

### 3. Delete Notification

**Endpoint**: `DELETE /notifications/{notificationId}`

**Description**: Delete a notification permanently

**Authentication**: Required (Bearer Token)

**Path Parameters**:
- `notificationId` (string, required): The unique identifier of the notification

**Request Headers**:
```
Authorization: Bearer {token}
X-Correlation-ID: {uuid}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Notification deleted successfully",
  "data": {
    "notificationId": "9f75b53e-681c-47f6-8afb-b1b99543410b",
    "deletedAt": "2026-06-03T09:05:00Z"
  },
  "errors": null
}
```


---

## Common Response Formats

### Standard Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data
  }
}
```

### Standard Error Response
```json
{
  "success": false,
  "message": "Description of error",
  "data": null,
  "errorCode": "ERROR_CODE",
  "traceId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Paginated Response
```json
{
  "success": true,
  "message": "Retrieved successfully",
  "data": {
    "items": [],
    "pagination": {
      "pageNumber": 1,
      "pageSize": 10,
      "totalRecords": 45,
      "totalPages": 5
    }
  }
}
```

---

## Error Codes & Messages

| HTTP Code | Error Code | Message | Description |
|-----------|-----------|---------|-------------|
| 400 | VALIDATION_ERROR | Validation failed | Request validation error |
| 401 | INVALID_CREDENTIALS | Invalid email or password | Authentication failed |
| 401 | INVALID_TOKEN | Token is invalid or expired | JWT token validation failed |
| 401 | UNAUTHORIZED | Unauthorized access | User doesn't have required permissions |
| 404 | NOT_FOUND | Resource not found | Requested resource doesn't exist |
| 404 | EMPLOYEE_NOT_FOUND | Employee not found | Employee ID not found |
| 404 | LEAVE_NOT_FOUND | Leave request not found | Leave ID not found |
| 409 | CONFLICT | Resource conflict | Duplicate resource or conflicting state |
| 400 | INSUFFICIENT_LEAVE_BALANCE | Insufficient leave balance | Not enough leave days available |
| 400 | INVALID_DATE_RANGE | Invalid date range | Start date after end date |
| 500 | INTERNAL_ERROR | Internal server error | Server error |
| 503 | SERVICE_UNAVAILABLE | Service temporarily unavailable | Service is down |

---

## Common Headers

### Request Headers (All Endpoints)
```
Content-Type: application/json
Authorization: Bearer {token}
X-Correlation-ID: {uuid}
```

### Response Headers
```
Content-Type: application/json
X-Correlation-ID: {uuid}
X-Response-Time: {milliseconds}
```

---

## Rate Limiting

**Limits per minute**:
- Authentication endpoints: 10 requests
- Other endpoints: 100 requests
- Admin endpoints: 200 requests

**Response Headers**:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1717097400
```

**Error Response** (429 Too Many Requests):
```json
{
  "success": false,
  "message": "Too many requests. Please try again later.",
  "errorCode": "RATE_LIMIT_EXCEEDED",
  "data": {
    "retryAfter": 60
  }
}
```

---

## Authentication Details

### JWT Token Structure
```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "9b0fb26a-ea43-4a62-9e79-28b6e53b44c8",
  "email": "john.doe@company.com",
  "roles": ["Employee", "Approver"],
  "iat": 1717097400,
  "exp": 1717183800
}
```

### Bearer Token Format
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5YjBmYjI2YS1lYTQzLTRhNjItOWU3OS0yOGI2ZTUzYjQ0YzgiLCJlbWFpbCI6ImpvaG4uZG9lQGNvbXBhbnkuY29tIiwicm9sZXMiOlsiRW1wbG95ZWUiXSwiaWF0IjoxNzE3MDk3NDAwLCJleHAiOjE3MTcxODM4MDB9.signature
```

---

## Testing with cURL

### Login Example
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@company.com",
    "password": "SecurePassword123!"
  }'
```

### Get Employees Example
```bash
curl -X GET http://localhost:5000/api/employees \
  -H "Authorization: Bearer {token}" \
  -H "X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000"
```

### Submit Leave Example
```bash
curl -X POST http://localhost:5000/api/leaves \
  -H "Authorization: Bearer {token}" \
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

---

## Versioning

- **Current API Version**: v1
- **URL Pattern**: `/api/v1/resource`
- **Backward Compatibility**: Maintained for at least 2 versions
- **Deprecation Notice**: 3 months warning before removing deprecated endpoints

---

**Document Version**: 1.0  
**Last Updated**: May 30, 2026  
**Next Review**: June 30, 2026
