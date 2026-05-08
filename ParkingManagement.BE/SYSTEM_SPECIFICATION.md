# 🏗️ PARKING MANAGEMENT SYSTEM - COMPREHENSIVE SPECIFICATION

**Version:** 2.0  
**Status:** Ready for Implementation (Giai Đoạn 4)  
**Date:** 2024-05-05

---

## 📋 TABLE OF CONTENTS

1. [System Overview](#system-overview)
2. [Role-Based Access Control (RBAC)](#role-based-access-control)
3. [Customer Features](#customer-features)
4. [Manager Features](#manager-features)
5. [Employee Features](#employee-features)
6. [Data Models](#data-models)
7. [API Endpoints Map](#api-endpoints-map)
8. [Database Schema](#database-schema)

---

## 🎯 SYSTEM OVERVIEW

### Purpose
Parking Management System for automated vehicle parking operations with role-based access control for 3 user types.

### Target Users
- **👤 Customers:** Vehicle owners seeking parking
- **👨‍💼 Managers:** Parking lot supervisors and administrators
- **👨‍💻 Employees:** Parking lot staff handling check-in/out

### Key Features
- User authentication & authorization
- Parking slot management
- Ticket (check-in/check-out) processing
- Monthly subscription tickets
- Reservation system
- Payment processing
- Reports & analytics
- Account management

---

## 🔐 ROLE-BASED ACCESS CONTROL

### 3 Main Roles

| Role | Description | Access Level |
|------|-------------|--------------|
| **Customer** | Vehicle owner | Basic |
| **Manager** | System administrator | Full |
| **Employee** | Parking lot staff | Limited |

---

# 🛍️ CUSTOMER FEATURES

## 1. Authentication

### 1.1 User Registration
**Endpoint:** `POST /api/auth/register/customer`  
**Request:**
```json
{
  "fullName": "Nguyen Van A",
  "email": "customer@example.com",
  "password": "SecurePass123!",
  "phoneNumber": "0912345678"
}
```

**Response:** 
```json
{
  "success": true,
  "message": "OTP sent to email",
  "data": {
    "accountId": "ACC001",
    "email": "customer@example.com"
  }
}
```

**Workflow:**
1. Validate input (email format, password strength)
2. Check if email already exists
3. Create temporary account
4. Generate OTP and send to email
5. Return temporary account info

**Validation Rules:**
- Email: Valid email format, unique
- Password: Min 8 chars, 1 uppercase, 1 number, 1 special char
- Full Name: Required, 2-100 chars
- Phone: Valid Vietnamese phone format

---

### 1.2 OTP Verification
**Endpoint:** `POST /api/auth/verify-otp`  
**Request:**
```json
{
  "email": "customer@example.com",
  "otp": "123456"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Account verified successfully",
  "data": {
    "token": "eyJhbGc...",
    "refreshToken": "eyJhbGc...",
    "expiresIn": 3600
  }
}
```

**Workflow:**
1. Verify OTP validity (6 digits, expires in 10 min)
2. Check OTP matches sent OTP
3. Activate account
4. Generate JWT token
5. Return auth tokens

---

### 1.3 User Login
**Endpoint:** `POST /api/auth/login`  
**Request:**
```json
{
  "email": "customer@example.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGc...",
    "refreshToken": "eyJhbGc...",
    "accountId": "ACC001",
    "role": "Customer",
    "relatedId": "CUST001",
    "fullName": "Nguyen Van A",
    "expiresIn": 3600
  }
}
```

**Workflow:**
1. Validate email & password
2. Check account exists and active
3. Generate JWT token (1 hour expiry)
4. Generate refresh token (7 days expiry)
5. Return tokens and user info

**Token Claims:**
```
{
  "sub": "ACC001",
  "email": "customer@example.com",
  "role": "Customer",
  "relatedId": "CUST001",
  "iat": timestamp,
  "exp": timestamp + 3600
}
```

---

## 2. Account Management

### 2.1 Get Account Profile
**Endpoint:** `GET /api/auth/me`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "accountId": "ACC001",
    "email": "customer@example.com",
    "role": "Customer",
    "customerId": "CUST001",
    "fullName": "Nguyen Van A",
    "phoneNumber": "0912345678",
    "avatarUrl": null,
    "createdAt": "2024-05-05T10:00:00Z",
    "lastLoginAt": "2024-05-05T15:30:00Z",
    "isActive": true
  }
}
```

---

### 2.2 Update Account Information
**Endpoint:** `PUT /api/customers/{customerId}`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "fullName": "Nguyen Van A Updated",
  "phoneNumber": "0987654321",
  "avatarUrl": "https://example.com/avatar.jpg"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Account updated successfully",
  "data": {
    "customerId": "CUST001",
    "fullName": "Nguyen Van A Updated",
    "phoneNumber": "0987654321",
    "updatedAt": "2024-05-05T16:00:00Z"
  }
}
```

**Allowed Fields:**
- Full Name (2-100 chars)
- Phone Number (Valid format)
- Avatar URL (Image URL)

---

### 2.3 Change Password
**Endpoint:** `POST /api/auth/change-password`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!",
  "confirmPassword": "NewPass456!"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

**Validation:**
- Current password must match
- New password must be different from current
- New password must meet complexity rules
- Confirmation must match new password

---

### 2.4 Forgot Password
**Endpoint:** `POST /api/auth/forgot-password`  
**Request:**
```json
{
  "email": "customer@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Password reset link sent to email"
}
```

**Workflow:**
1. Check email exists
2. Generate reset token (valid 30 min)
3. Send reset link via email
4. Return success message

---

## 3. Reservations Management

### 3.1 Get Reservation List
**Endpoint:** `GET /api/reservations?status=pending&page=1&pageSize=10`  
**Headers:** `Authorization: Bearer {token}`  
**Query Parameters:**
- `status`: pending, confirmed, checked_in, checked_out, cancelled
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `sortBy`: createdAt, reservationDate (default: createdAt)
- `sortOrder`: asc, desc (default: desc)

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "reservationId": "RES001",
        "customerId": "CUST001",
        "slotId": "SLOT001",
        "vehicleType": "Car",
        "vehiclePlate": "ABC123",
        "status": "pending",
        "reservationDate": "2024-05-10T08:00:00Z",
        "checkInTime": null,
        "checkOutTime": null,
        "expectedDuration": 480,
        "estimatedFee": 50000,
        "actualFee": null,
        "notes": "Reserved for meeting",
        "createdAt": "2024-05-05T15:30:00Z",
        "updatedAt": "2024-05-05T15:30:00Z"
      }
    ],
    "totalCount": 25,
    "pageCount": 3,
    "currentPage": 1,
    "pageSize": 10
  }
}
```

---

### 3.2 Get Reservation Detail
**Endpoint:** `GET /api/reservations/{reservationId}`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "reservationId": "RES001",
    "customerId": "CUST001",
    "slotId": "SLOT001",
    "slotNumber": "A-101",
    "slotLocation": "Floor 1, Block A",
    "vehicleType": "Car",
    "vehiclePlate": "ABC123",
    "status": "pending",
    "reservationDate": "2024-05-10T08:00:00Z",
    "checkInTime": null,
    "checkOutTime": null,
    "expectedDuration": 480,
    "estimatedFee": 50000,
    "actualFee": null,
    "notes": "Reserved for meeting",
    "createdAt": "2024-05-05T15:30:00Z",
    "updatedAt": "2024-05-05T15:30:00Z",
    "parking": {
      "parkingName": "Downtown Parking",
      "address": "123 Main St"
    }
  }
}
```

---

### 3.3 Create New Reservation
**Endpoint:** `POST /api/reservations`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "vehiclePlate": "ABC123",
  "vehicleType": "Car",
  "reservationDate": "2024-05-10T08:00:00Z",
  "expectedDuration": 480,
  "notes": "Reserved for meeting"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Reservation created successfully",
  "data": {
    "reservationId": "RES001",
    "slotId": "SLOT001",
    "slotNumber": "A-101",
    "status": "pending",
    "estimatedFee": 50000,
    "qrCode": "https://example.com/qr/RES001"
  }
}
```

**Workflow:**
1. Validate vehicle info (plate format, type)
2. Validate reservation date (future date only)
3. Find available slots matching criteria
4. Auto-assign best available slot
5. Generate QR code
6. Create reservation record
7. Send confirmation email

**Validation Rules:**
- Vehicle plate: Valid format
- Vehicle type: Car, Motorcycle, Truck
- Reservation date: Must be future date, at least 1 hour from now
- Expected duration: 30 min - 720 min (12 hours)

---

### 3.4 Cancel Reservation
**Endpoint:** `DELETE /api/reservations/{reservationId}`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "message": "Reservation cancelled successfully",
  "data": {
    "reservationId": "RES001",
    "status": "cancelled",
    "cancellationReason": null,
    "refundAmount": 50000,
    "cancelledAt": "2024-05-05T16:00:00Z"
  }
}
```

**Rules:**
- Only pending/confirmed reservations can be cancelled
- Can cancel up to 1 hour before reservation time
- Full refund if cancelled > 30 min before
- No refund if cancelled < 30 min before
- Checked-in/checked-out reservations cannot be cancelled

---

### 3.5 Get Available Slots
**Endpoint:** `GET /api/reservations/available-slots?vehicleType=Car&date=2024-05-10&time=08:00`  
**Headers:** `Authorization: Bearer {token}`  
**Query Parameters:**
- `vehicleType`: Car, Motorcycle, Truck
- `date`: YYYY-MM-DD
- `time`: HH:MM (optional)
- `duration`: Minutes (default: 480)

**Response:**
```json
{
  "success": true,
  "data": {
    "availableSlots": [
      {
        "slotId": "SLOT001",
        "slotNumber": "A-101",
        "location": "Floor 1, Block A",
        "type": "Standard",
        "price": 50000,
        "estimatedFee": 50000,
        "distance": 50
      },
      {
        "slotId": "SLOT002",
        "slotNumber": "A-102",
        "location": "Floor 1, Block A",
        "type": "Standard",
        "price": 50000,
        "estimatedFee": 50000,
        "distance": 45
      }
    ],
    "totalAvailable": 15,
    "requestedDate": "2024-05-10",
    "requestedTime": "08:00"
  }
}
```

---

## 4. Tickets (Parking Sessions) Management

### 4.1 Get Tickets List
**Endpoint:** `GET /api/tickets?status=completed&month=05&year=2024&page=1`  
**Headers:** `Authorization: Bearer {token}`  
**Query Parameters:**
- `status`: active, completed, cancelled
- `month`: MM (optional)
- `year`: YYYY (optional)
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20)
- `sortBy`: checkInTime, fee
- `sortOrder`: asc, desc

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "ticketId": "TKT001",
        "customerId": "CUST001",
        "slotId": "SLOT001",
        "slotNumber": "A-101",
        "vehiclePlate": "ABC123",
        "vehicleType": "Car",
        "checkInTime": "2024-05-05T08:30:00Z",
        "checkOutTime": "2024-05-05T10:15:00Z",
        "duration": 105,
        "fee": 52500,
        "status": "completed",
        "paymentStatus": "paid",
        "paymentMethod": "card",
        "notes": null,
        "createdAt": "2024-05-05T08:30:00Z"
      }
    ],
    "totalCount": 45,
    "pageCount": 3,
    "currentPage": 1,
    "pageSize": 20,
    "summary": {
      "totalTickets": 45,
      "totalFee": 2250000,
      "averageDuration": 95,
      "completedCount": 40,
      "cancelledCount": 5
    }
  }
}
```

---

### 4.2 Get Ticket Detail
**Endpoint:** `GET /api/tickets/{ticketId}`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "ticketId": "TKT001",
    "customerId": "CUST001",
    "slotId": "SLOT001",
    "slotNumber": "A-101",
    "slotLocation": "Floor 1, Block A",
    "vehiclePlate": "ABC123",
    "vehicleType": "Car",
    "checkInTime": "2024-05-05T08:30:00Z",
    "checkOutTime": "2024-05-05T10:15:00Z",
    "duration": 105,
    "pricePerHour": 30000,
    "fee": 52500,
    "status": "completed",
    "paymentStatus": "paid",
    "paymentMethod": "card",
    "paymentTime": "2024-05-05T10:15:30Z",
    "transactionId": "PAY001",
    "notes": null,
    "qrCode": "https://example.com/qr/TKT001",
    "createdAt": "2024-05-05T08:30:00Z"
  }
}
```

---

### 4.3 Payment History
**Endpoint:** `GET /api/tickets/{ticketId}/payment-history`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "ticketId": "TKT001",
    "payments": [
      {
        "paymentId": "PAY001",
        "amount": 52500,
        "method": "card",
        "status": "success",
        "transactionId": "TXN123456",
        "paidAt": "2024-05-05T10:15:30Z",
        "reference": "Parking Fee - May 5"
      }
    ],
    "totalPaid": 52500,
    "remainingFee": 0,
    "status": "fully_paid"
  }
}
```

---

## 5. Monthly Tickets Management

### 5.1 Get Monthly Tickets
**Endpoint:** `GET /api/monthly-tickets/customer/{customerId}?page=1`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "monthlyTicketId": "MT001",
        "customerId": "CUST001",
        "vehiclePlate": "ABC123",
        "vehicleType": "Car",
        "ticketType": "monthly",
        "status": "active",
        "purchaseDate": "2024-04-05T00:00:00Z",
        "startDate": "2024-04-05T00:00:00Z",
        "expiryDate": "2024-05-04T23:59:59Z",
        "remainingDays": 28,
        "price": 500000,
        "daysUsed": 2,
        "visitsUsed": 5,
        "renewalDate": null,
        "autoRenew": false,
        "createdAt": "2024-04-05T10:00:00Z"
      }
    ],
    "totalCount": 3,
    "pageCount": 1,
    "activeCount": 2,
    "expiredCount": 1
  }
}
```

---

### 5.2 Get Monthly Ticket Detail
**Endpoint:** `GET /api/monthly-tickets/{monthlyTicketId}`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "monthlyTicketId": "MT001",
    "customerId": "CUST001",
    "vehiclePlate": "ABC123",
    "vehicleType": "Car",
    "ticketType": "monthly",
    "status": "active",
    "purchaseDate": "2024-04-05T00:00:00Z",
    "startDate": "2024-04-05T00:00:00Z",
    "expiryDate": "2024-05-04T23:59:59Z",
    "remainingDays": 28,
    "price": 500000,
    "daysUsed": 2,
    "visitsUsed": 5,
    "maxVisitsPerDay": 3,
    "usageDetail": [
      {
        "date": "2024-05-05",
        "visitCount": 2
      },
      {
        "date": "2024-05-04",
        "visitCount": 3
      }
    ],
    "autoRenew": false,
    "renewalDate": null,
    "qrCode": "https://example.com/qr/MT001"
  }
}
```

---

### 5.3 Get Monthly Ticket Pricing
**Endpoint:** `GET /api/pricing/monthly`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "packages": [
      {
        "packageId": "PKG001",
        "name": "Monthly - Car",
        "vehicleType": "Car",
        "duration": 30,
        "price": 500000,
        "maxVisitsPerDay": 3,
        "description": "Unlimited parking for 30 days",
        "features": [
          "Unlimited parking",
          "Priority slots",
          "24/7 access"
        ]
      },
      {
        "packageId": "PKG002",
        "name": "Monthly - Motorcycle",
        "vehicleType": "Motorcycle",
        "duration": 30,
        "price": 200000,
        "maxVisitsPerDay": 5,
        "description": "Unlimited parking for 30 days"
      }
    ]
  }
}
```

---

### 5.4 Purchase Monthly Ticket
**Endpoint:** `POST /api/monthly-tickets`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "packageId": "PKG001",
  "vehiclePlate": "ABC123",
  "vehicleType": "Car",
  "paymentMethod": "card",
  "autoRenew": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Monthly ticket purchased successfully",
  "data": {
    "monthlyTicketId": "MT001",
    "customerId": "CUST001",
    "startDate": "2024-05-05T00:00:00Z",
    "expiryDate": "2024-06-04T23:59:59Z",
    "price": 500000,
    "paymentId": "PAY001",
    "qrCode": "https://example.com/qr/MT001"
  }
}
```

---

### 5.5 Renew Monthly Ticket
**Endpoint:** `POST /api/monthly-tickets/{monthlyTicketId}/renew`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "paymentMethod": "card"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Monthly ticket renewed successfully",
  "data": {
    "monthlyTicketId": "MT001",
    "newStartDate": "2024-05-05T00:00:00Z",
    "newExpiryDate": "2024-06-04T23:59:59Z",
    "renewalPrice": 500000,
    "paymentId": "PAY002"
  }
}
```

---

# 👨‍💼 MANAGER FEATURES

## 1. Manager Authentication & Account

### 1.1 Manager Login
**Endpoint:** `POST /api/auth/login`  
**Request:**
```json
{
  "email": "manager@example.com",
  "password": "ManagerPass123!"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGc...",
    "refreshToken": "eyJhbGc...",
    "accountId": "ACC002",
    "role": "Manager",
    "relatedId": "MGR001",
    "fullName": "Manager Name",
    "expiresIn": 3600
  }
}
```

---

### 1.2 Get Manager Profile
**Endpoint:** `GET /api/auth/me`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "accountId": "ACC002",
    "managerId": "MGR001",
    "email": "manager@example.com",
    "role": "Manager",
    "fullName": "Manager Name",
    "phoneNumber": "0912345678",
    "createdAt": "2024-01-01T00:00:00Z",
    "lastLoginAt": "2024-05-05T15:30:00Z",
    "isActive": true
  }
}
```

---

### 1.3 Update Manager Profile
**Endpoint:** `PUT /api/managers/{managerId}`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "fullName": "Manager Name Updated",
  "phoneNumber": "0987654321"
}
```

**Response:** Success confirmation

---

### 1.4 Activity Log
**Endpoint:** `GET /api/managers/{managerId}/activity-log?page=1&pageSize=50`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "logId": "LOG001",
        "action": "UPDATE_SLOT_STATUS",
        "description": "Updated slot A-101 status to Occupied",
        "affectedEntity": "SLOT001",
        "timestamp": "2024-05-05T15:30:00Z",
        "ipAddress": "192.168.1.100",
        "userAgent": "Mozilla/5.0..."
      },
      {
        "logId": "LOG002",
        "action": "DELETE_EMPLOYEE",
        "description": "Deleted employee EMP001",
        "affectedEntity": "EMP001",
        "timestamp": "2024-05-05T14:20:00Z",
        "ipAddress": "192.168.1.100",
        "userAgent": "Mozilla/5.0..."
      }
    ],
    "totalCount": 250,
    "pageCount": 5,
    "currentPage": 1
  }
}
```

---

## 2. Ticket Management (Manager View)

### 2.1 Get All Tickets
**Endpoint:** `GET /api/tickets?page=1&pageSize=50&dateFrom=2024-05-01&dateTo=2024-05-31`  
**Headers:** `Authorization: Bearer {token}`  
**Query Parameters:**
- `page`: Page number
- `pageSize`: Items per page
- `dateFrom`: Start date
- `dateTo`: End date
- `status`: active, completed, cancelled
- `vehicleType`: All, Car, Motorcycle
- `searchBy`: plate, customerId
- `searchValue`: Search value

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "ticketId": "TKT001",
        "customerId": "CUST001",
        "customerName": "Nguyen Van A",
        "slotNumber": "A-101",
        "vehiclePlate": "ABC123",
        "vehicleType": "Car",
        "checkInTime": "2024-05-05T08:30:00Z",
        "checkOutTime": "2024-05-05T10:15:00Z",
        "duration": 105,
        "fee": 52500,
        "paymentStatus": "paid",
        "status": "completed"
      }
    ],
    "summary": {
      "totalTickets": 450,
      "totalRevenue": 22500000,
      "completedTickets": 440,
      "cancelledTickets": 10,
      "averageFee": 50000,
      "averageDuration": 95
    }
  }
}
```

---

### 2.2 Find Ticket by Details
**Endpoint:** `GET /api/tickets/search?plate=ABC123&customerId=CUST001`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "ticketId": "TKT001",
        "customerName": "Nguyen Van A",
        "vehiclePlate": "ABC123",
        "checkInTime": "2024-05-05T08:30:00Z",
        "status": "completed",
        "fee": 52500
      }
    ],
    "totalFound": 15
  }
}
```

---

### 2.3 Manage Ticket Pricing
**Endpoint:** `GET /api/pricing`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "pricingId": "PRC001",
        "vehicleType": "Car",
        "pricePerHour": 30000,
        "pricePerDay": 200000,
        "minFee": 10000,
        "maxFee": 500000,
        "effectiveDate": "2024-05-01T00:00:00Z",
        "endDate": null,
        "isActive": true,
        "description": "Standard car parking rate"
      }
    ]
  }
}
```

---

### 2.4 Update Ticket Pricing
**Endpoint:** `PUT /api/pricing/{pricingId}`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "pricePerHour": 35000,
  "pricePerDay": 220000,
  "minFee": 12000,
  "maxFee": 550000,
  "effectiveDate": "2024-05-06T00:00:00Z"
}
```

**Response:** Success confirmation

---

## 3. Employee Management

### 3.1 Get Employee List
**Endpoint:** `GET /api/employees?page=1&pageSize=20&status=active`  
**Headers:** `Authorization: Bearer {token}`  
**Query Parameters:**
- `page`: Page number
- `pageSize`: Items per page
- `status`: active, inactive, deleted
- `search`: Name, email, ID

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "employeeId": "EMP001",
        "accountId": "ACC003",
        "fullName": "Employee Name",
        "email": "emp@example.com",
        "phoneNumber": "0912345678",
        "position": "Parking Attendant",
        "hireDate": "2024-01-01T00:00:00Z",
        "status": "active",
        "department": "Operations",
        "shift": "morning",
        "totalShifts": 45,
        "totalTicketsProcessed": 250,
        "totalRevenue": 12500000,
        "createdAt": "2024-01-01T10:00:00Z"
      }
    ],
    "totalCount": 15,
    "summary": {
      "totalEmployees": 15,
      "activeEmployees": 13,
      "inactiveEmployees": 2
    }
  }
}
```

---

### 3.2 Get Employee Detail & Statistics
**Endpoint:** `GET /api/employees/{employeeId}/detail`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "employeeId": "EMP001",
    "accountId": "ACC003",
    "fullName": "Employee Name",
    "email": "emp@example.com",
    "phoneNumber": "0912345678",
    "position": "Parking Attendant",
    "hireDate": "2024-01-01T00:00:00Z",
    "status": "active",
    "department": "Operations",
    "shift": "morning",
    "personalInfo": {
      "dateOfBirth": "1990-05-15",
      "address": "123 Main St",
      "idCard": "123456789",
      "bankAccount": "1234567890"
    },
    "statistics": {
      "totalShifts": 45,
      "totalHours": 360,
      "totalTicketsProcessed": 250,
      "totalRevenue": 12500000,
      "averageRevenuePerShift": 277777,
      "onTimeRate": 95,
      "absenceRate": 2
    },
    "recentActivity": [
      {
        "date": "2024-05-05",
        "shiftsWorked": 1,
        "ticketsProcessed": 12,
        "revenue": 600000
      }
    ]
  }
}
```

---

### 3.3 Search Employee
**Endpoint:** `GET /api/employees/search?name=Employee&email=emp@example.com`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "employeeId": "EMP001",
        "fullName": "Employee Name",
        "email": "emp@example.com",
        "status": "active"
      }
    ],
    "totalFound": 1
  }
}
```

---

### 3.4 Invite Employee
**Endpoint:** `POST /api/employees/invite`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "email": "newemp@example.com",
  "fullName": "New Employee",
  "position": "Parking Attendant",
  "department": "Operations"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Invitation sent successfully",
  "data": {
    "inviteToken": "inv_abc123def456",
    "expiryTime": 86400,
    "inviteLink": "https://example.com/register?token=inv_abc123def456"
  }
}
```

---

### 3.5 Update Employee
**Endpoint:** `PUT /api/employees/{employeeId}`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "fullName": "Updated Name",
  "phoneNumber": "0987654321",
  "position": "Senior Attendant",
  "shift": "afternoon",
  "department": "Operations"
}
```

**Response:** Success confirmation

---

### 3.6 Delete Employee (Soft Delete)
**Endpoint:** `DELETE /api/employees/{employeeId}`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "reason": "Employee resigned"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Employee account deactivated",
  "data": {
    "employeeId": "EMP001",
    "status": "deleted",
    "deactivatedAt": "2024-05-05T16:00:00Z"
  }
}
```

---

## 4. Parking Slots Management

### 4.1 Get All Parking Slots
**Endpoint:** `GET /api/parking-slots?page=1&pageSize=50&status=all`  
**Headers:** `Authorization: Bearer {token}`  
**Query Parameters:**
- `page`: Page number
- `pageSize`: Items per page
- `status`: all, available, occupied, maintenance, reserved
- `slotType`: Standard, VIP, Disabled
- `floor`: Floor number

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "slotId": "SLOT001",
        "slotNumber": "A-101",
        "floor": 1,
        "block": "A",
        "type": "Standard",
        "vehicleType": "Car",
        "status": "available",
        "location": "Floor 1, Block A",
        "price": 30000,
        "lastUpdated": "2024-05-05T15:30:00Z",
        "currentOccupant": null,
        "reservedBy": null
      }
    ],
    "summary": {
      "totalSlots": 100,
      "availableSlots": 45,
      "occupiedSlots": 50,
      "reservedSlots": 3,
      "maintenanceSlots": 2,
      "occupancyRate": 50
    }
  }
}
```

---

### 4.2 Get Slot Detail
**Endpoint:** `GET /api/parking-slots/{slotId}`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "slotId": "SLOT001",
    "slotNumber": "A-101",
    "floor": 1,
    "block": "A",
    "type": "Standard",
    "vehicleType": "Car",
    "status": "occupied",
    "location": "Floor 1, Block A",
    "price": 30000,
    "lastUpdated": "2024-05-05T15:30:00Z",
    "currentOccupancy": {
      "ticketId": "TKT001",
      "vehiclePlate": "ABC123",
      "checkInTime": "2024-05-05T14:30:00Z",
      "expectedCheckOut": "2024-05-05T16:30:00Z",
      "estimatedFee": 60000
    },
    "auditLog": [
      {
        "timestamp": "2024-05-05T14:30:00Z",
        "action": "CHECK_IN",
        "oldStatus": "available",
        "newStatus": "occupied",
        "ticketId": "TKT001"
      }
    ]
  }
}
```

---

### 4.3 Update Slot Status
**Endpoint:** `PATCH /api/parking-slots/{slotId}/status`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "status": "maintenance",
  "reason": "Cleaning and maintenance"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Slot status updated successfully",
  "data": {
    "slotId": "SLOT001",
    "status": "maintenance",
    "updatedAt": "2024-05-05T16:00:00Z"
  }
}
```

---

### 4.4 Get Slots Status Summary
**Endpoint:** `GET /api/parking-slots/summary`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "totalSlots": 100,
    "availableCount": 45,
    "occupiedCount": 50,
    "reservedCount": 3,
    "maintenanceCount": 2,
    "occupancyRate": 50,
    "byFloor": [
      {
        "floor": 1,
        "totalSlots": 25,
        "available": 10,
        "occupied": 13,
        "reserved": 1,
        "maintenance": 1,
        "occupancyRate": 56
      }
    ],
    "byType": [
      {
        "type": "Standard",
        "totalSlots": 80,
        "available": 40,
        "occupied": 35,
        "occupancyRate": 44
      }
    ]
  }
}
```

---

### 4.5 Get Parking Report
**Endpoint:** `GET /api/parking-slots/report?dateFrom=2024-05-01&dateTo=2024-05-31`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "period": "2024-05-01 to 2024-05-31",
    "totalCheckins": 450,
    "totalCheckouts": 448,
    "totalRevenue": 22500000,
    "averageOccupancy": 48,
    "peakHours": "12:00 - 14:00",
    "peakOccupancy": 92,
    "byVehicleType": [
      {
        "vehicleType": "Car",
        "checkins": 350,
        "revenue": 17500000,
        "averageStayTime": 105
      },
      {
        "vehicleType": "Motorcycle",
        "checkins": 100,
        "revenue": 5000000,
        "averageStayTime": 110
      }
    ],
    "dailyStats": [
      {
        "date": "2024-05-05",
        "checkins": 20,
        "checkouts": 20,
        "revenue": 1000000,
        "averageOccupancy": 45
      }
    ]
  }
}
```

---

## 5. Reports & Analytics

### 5.1 Dashboard
**Endpoint:** `GET /api/reports/manager/dashboard`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "overview": {
      "totalRevenue": 50000000,
      "monthlyRevenue": 4200000,
      "todayRevenue": 150000,
      "totalTickets": 1000,
      "monthlyTickets": 85,
      "todayTickets": 12,
      "occupancyRate": 48,
      "activeEmployees": 13
    },
    "revenueChart": {
      "period": "last_30_days",
      "data": [
        {
          "date": "2024-05-05",
          "revenue": 150000,
          "tickets": 12
        }
      ]
    },
    "occupancyChart": {
      "byHour": [
        {
          "hour": "08:00",
          "occupancy": 35
        }
      ]
    },
    "topVehicles": [
      {
        "plate": "ABC123",
        "type": "Car",
        "visits": 25,
        "totalSpent": 1250000
      }
    ]
  }
}
```

---

### 5.2 Revenue Report
**Endpoint:** `POST /api/reports/manager/revenue`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "period": "monthly",
  "dateFrom": "2024-05-01",
  "dateTo": "2024-05-31",
  "groupBy": "day"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "period": "2024-05-01 to 2024-05-31",
    "totalRevenue": 4200000,
    "totalTickets": 85,
    "averagePerTicket": 49412,
    "byDay": [
      {
        "date": "2024-05-01",
        "revenue": 150000,
        "tickets": 12,
        "average": 12500
      }
    ],
    "byVehicleType": [
      {
        "type": "Car",
        "revenue": 3500000,
        "tickets": 70,
        "percentage": 83
      }
    ],
    "topDays": [
      {
        "date": "2024-05-10",
        "revenue": 250000,
        "tickets": 20,
        "trend": "up"
      }
    ]
  }
}
```

---

### 5.3 Customer Report
**Endpoint:** `GET /api/reports/manager/customers?dateFrom=2024-05-01&dateTo=2024-05-31`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "totalCustomers": 250,
    "newCustomers": 15,
    "activeCustomers": 200,
    "topCustomers": [
      {
        "customerId": "CUST001",
        "name": "Nguyen Van A",
        "visits": 45,
        "totalSpent": 2250000,
        "memberSince": "2024-01-01"
      }
    ],
    "monthlySubscribers": 50,
    "monthlyRevenue": 25000000,
    "customerSegmentation": {
      "highValue": 20,
      "regular": 150,
      "occasional": 80
    }
  }
}
```

---

### 5.4 Comprehensive Report
**Endpoint:** `GET /api/reports/manager/comprehensive?period=monthly&month=05&year=2024`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "period": "May 2024",
    "financialSummary": {
      "totalRevenue": 4200000,
      "ticketRevenue": 2100000,
      "monthlySubscriptions": 2100000,
      "expenses": 500000,
      "profit": 3700000
    },
    "operationSummary": {
      "totalTickets": 85,
      "totalCustomers": 250,
      "totalEmployees": 13,
      "averageOccupancy": 48,
      "peakHour": "12:00"
    },
    "employeeSummary": {
      "totalShifts": 200,
      "averageTicketsPerShift": 5,
      "topPerformer": "EMP001",
      "topPerformerRevenue": 600000
    }
  }
}
```

---

# 👨‍💻 EMPLOYEE FEATURES

## 1. Employee Authentication & Account

### 1.1 Employee Login
**Endpoint:** `POST /api/auth/login`  
**Request:**
```json
{
  "email": "emp@example.com",
  "password": "EmpPass123!"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGc...",
    "refreshToken": "eyJhbGc...",
    "accountId": "ACC003",
    "role": "Employee",
    "relatedId": "EMP001",
    "fullName": "Employee Name",
    "expiresIn": 3600
  }
}
```

---

### 1.2 Get Employee Profile
**Endpoint:** `GET /api/auth/me`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "accountId": "ACC003",
    "employeeId": "EMP001",
    "email": "emp@example.com",
    "fullName": "Employee Name",
    "phoneNumber": "0912345678",
    "position": "Parking Attendant",
    "shift": "morning",
    "hireDate": "2024-01-01T00:00:00Z",
    "isActive": true
  }
}
```

---

### 1.3 Update Employee Profile
**Endpoint:** `PUT /api/employees/{employeeId}`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "fullName": "Updated Name",
  "phoneNumber": "0987654321"
}
```

---

### 1.4 Change Password
**Endpoint:** `POST /api/auth/change-password`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!",
  "confirmPassword": "NewPass456!"
}
```

---

## 2. Customer Management (Employee View)

### 2.1 Get Customers List
**Endpoint:** `GET /api/employees/customers?page=1&pageSize=50&search=name`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "customerId": "CUST001",
        "name": "Nguyen Van A",
        "email": "customer@example.com",
        "phoneNumber": "0912345678",
        "totalVisits": 45,
        "totalSpent": 2250000,
        "lastVisit": "2024-05-05T15:30:00Z",
        "monthlySubscriber": true
      }
    ],
    "totalCount": 250,
    "pageCount": 5
  }
}
```

---

### 2.2 Search Customer
**Endpoint:** `GET /api/employees/customers/search?name=Nguyen&email=customer@example.com`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "customerId": "CUST001",
        "name": "Nguyen Van A",
        "email": "customer@example.com",
        "totalVisits": 45
      }
    ],
    "totalFound": 1
  }
}
```

---

## 3. Ticket Management (Employee Operations)

### 3.1 Get All Tickets
**Endpoint:** `GET /api/employees/tickets?page=1&pageSize=50&status=active`  
**Headers:** `Authorization: Bearer {token}`  
**Query Parameters:**
- `status`: active, completed
- `sortBy`: checkInTime, fee
- `sortOrder`: asc, desc

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "ticketId": "TKT001",
        "vehiclePlate": "ABC123",
        "vehicleType": "Car",
        "checkInTime": "2024-05-05T08:30:00Z",
        "expectedCheckOut": "2024-05-05T10:30:00Z",
        "status": "active",
        "estimatedFee": 60000
      }
    ],
    "totalCount": 15,
    "activeTickets": 12,
    "completedToday": 8
  }
}
```

---

### 3.2 Search Ticket
**Endpoint:** `GET /api/employees/tickets/search?plate=ABC123`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "ticketId": "TKT001",
        "vehiclePlate": "ABC123",
        "checkInTime": "2024-05-05T08:30:00Z",
        "status": "active"
      }
    ],
    "totalFound": 1
  }
}
```

---

### 3.3 Create New Ticket
**Endpoint:** `POST /api/tickets`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "vehiclePlate": "XYZ789",
  "vehicleType": "Car",
  "slotId": "SLOT005",
  "notes": "Manual ticket creation"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Ticket created successfully",
  "data": {
    "ticketId": "TKT002",
    "vehiclePlate": "XYZ789",
    "slotNumber": "A-105",
    "checkInTime": "2024-05-05T16:00:00Z",
    "status": "active",
    "qrCode": "https://example.com/qr/TKT002"
  }
}
```

---

### 3.4 Check-In Process
**Endpoint:** `POST /api/tickets/checkin/confirm`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "vehiclePlate": "ABC123",
  "vehicleType": "Car",
  "notes": "Checked in by employee"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Check-in successful",
  "data": {
    "ticketId": "TKT001",
    "slotNumber": "A-101",
    "vehiclePlate": "ABC123",
    "checkInTime": "2024-05-05T08:30:00Z",
    "estimatedFee": 60000,
    "qrCode": "https://example.com/qr/TKT001"
  }
}
```

---

### 3.5 Check-Out & Payment Processing
**Endpoint:** `POST /api/tickets/checkout/confirm`  
**Headers:** `Authorization: Bearer {token}`  
**Request:**
```json
{
  "ticketId": "TKT001",
  "paymentMethod": "cash",
  "amount": 52500,
  "notes": "Payment received"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Check-out successful",
  "data": {
    "ticketId": "TKT001",
    "checkOutTime": "2024-05-05T10:15:00Z",
    "duration": 105,
    "fee": 52500,
    "paymentStatus": "paid",
    "paymentId": "PAY001",
    "change": 0
  }
}
```

---

## 4. Parking Slots Management (Employee View)

### 4.1 Get All Slots
**Endpoint:** `GET /api/employees/parking-slots?page=1&pageSize=50`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "slotId": "SLOT001",
        "slotNumber": "A-101",
        "floor": 1,
        "status": "occupied",
        "vehiclePlate": "ABC123",
        "checkInTime": "2024-05-05T08:30:00Z",
        "estimatedCheckOut": "2024-05-05T10:30:00Z"
      }
    ],
    "summary": {
      "totalSlots": 100,
      "available": 45,
      "occupied": 50,
      "maintenance": 5
    }
  }
}
```

---

### 4.2 Get Slot Detail
**Endpoint:** `GET /api/employees/parking-slots/{slotId}`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "slotId": "SLOT001",
    "slotNumber": "A-101",
    "floor": 1,
    "block": "A",
    "status": "occupied",
    "currentOccupant": {
      "ticketId": "TKT001",
      "vehiclePlate": "ABC123",
      "checkInTime": "2024-05-05T08:30:00Z",
      "expectedFee": 60000
    }
  }
}
```

---

## 5. Reports & Dashboard (Employee)

### 5.1 Employee Dashboard
**Endpoint:** `GET /api/reports/employee/{employeeId}/dashboard`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "today": {
      "shiftsWorked": 1,
      "totalTickets": 12,
      "totalRevenue": 600000,
      "averageFee": 50000,
      "startTime": "2024-05-05T08:00:00Z",
      "endTime": null,
      "hoursWorked": 7
    },
    "thisWeek": {
      "shiftsWorked": 5,
      "totalTickets": 65,
      "totalRevenue": 3250000,
      "averageTicketsPerShift": 13
    },
    "thisMonth": {
      "shiftsWorked": 20,
      "totalTickets": 280,
      "totalRevenue": 14000000,
      "rank": 2,
      "topPerformer": "EMP002",
      "topPerformerRevenue": 15000000
    },
    "performance": {
      "totalTickets": 500,
      "totalRevenue": 25000000,
      "averagePerShift": 25,
      "rating": 4.8
    }
  }
}
```

---

### 5.2 Attendance Report
**Endpoint:** `GET /api/reports/employee/{employeeId}/attendance?month=05&year=2024`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "period": "May 2024",
    "totalShifts": 20,
    "attendedShifts": 20,
    "absentShifts": 0,
    "attendanceRate": 100,
    "lateArrivals": 2,
    "earlyDepartures": 1,
    "scheduleDetail": [
      {
        "date": "2024-05-05",
        "shift": "morning",
        "scheduledTime": "08:00",
        "checkInTime": "08:05",
        "checkOutTime": "16:00",
        "status": "present",
        "hoursWorked": 8
      }
    ]
  }
}
```

---

### 5.3 Revenue Report
**Endpoint:** `GET /api/reports/employee/{employeeId}/revenue?dateFrom=2024-05-01&dateTo=2024-05-31`  
**Headers:** `Authorization: Bearer {token}`  
**Response:**
```json
{
  "success": true,
  "data": {
    "period": "2024-05-01 to 2024-05-31",
    "totalRevenue": 14000000,
    "totalTickets": 280,
    "averagePerTicket": 50000,
    "byDay": [
      {
        "date": "2024-05-05",
        "revenue": 600000,
        "tickets": 12,
        "average": 50000
      }
    ],
    "byVehicleType": [
      {
        "type": "Car",
        "revenue": 11500000,
        "tickets": 230,
        "percentage": 82
      }
    ],
    "trend": "up",
    "comparison": {
      "previousMonth": 12000000,
      "growth": 17
    }
  }
}
```

---

# 📊 DATA MODELS

## Common Models

### User Account
```
Account
├── accountId: string (PK)
├── email: string (unique)
├── passwordHash: string
├── role: enum (Customer, Manager, Employee)
├── relatedId: string (FK to Customer/Manager/Employee)
├── isActive: boolean
├── createdAt: datetime
├── lastLoginAt: datetime
├── updatedAt: datetime
```

### Customer
```
Customer
├── customerId: string (PK)
├── accountId: string (FK)
├── fullName: string
├── phoneNumber: string
├── email: string
├── avatarUrl: string
├── totalVisits: integer
├── totalSpent: decimal
├── monthlySubscriber: boolean
├── isActive: boolean
├── createdAt: datetime
├── updatedAt: datetime
```

### Manager
```
Manager
├── managerId: string (PK)
├── accountId: string (FK)
├── fullName: string
├── phoneNumber: string
├── email: string
├── createdAt: datetime
├── updatedAt: datetime
```

### Employee
```
Employee
├── employeeId: string (PK)
├── accountId: string (FK)
├── fullName: string
├── phoneNumber: string
├── email: string
├── position: string
├── hireDate: datetime
├── shift: enum (morning, afternoon, night)
├── department: string
├── totalShifts: integer
├── totalTicketsProcessed: integer
├── totalRevenue: decimal
├── isActive: boolean
├── createdAt: datetime
├── updatedAt: datetime
├── deletedAt: datetime (soft delete)
```

### Parking Slot
```
ParkingSlot
├── slotId: string (PK)
├── slotNumber: string (unique)
├── floor: integer
├── block: string
├── type: enum (Standard, VIP, Disabled)
├── vehicleType: enum (Car, Motorcycle, Truck)
├── status: enum (Available, Occupied, Reserved, Maintenance)
├── price: decimal
├── location: string
├── createdAt: datetime
├── updatedAt: datetime
```

### Ticket
```
Ticket
├── ticketId: string (PK)
├── customerId: string (FK)
├── slotId: string (FK)
├── vehiclePlate: string
├── vehicleType: enum
├── checkInTime: datetime
├── checkOutTime: datetime
├── duration: integer (minutes)
├── fee: decimal
├── paymentStatus: enum (pending, paid, refunded)
├── paymentMethod: enum (cash, card, transfer)
├── status: enum (active, completed, cancelled)
├── createdAt: datetime
├── updatedAt: datetime
```

### Monthly Ticket
```
MonthlyTicket
├── monthlyTicketId: string (PK)
├── customerId: string (FK)
├── vehiclePlate: string
├── vehicleType: enum
├── startDate: datetime
├── expiryDate: datetime
├── price: decimal
├── status: enum (active, expired, cancelled)
├── autoRenew: boolean
├── visitsUsed: integer
├── createdAt: datetime
├── updatedAt: datetime
```

### Reservation
```
Reservation
├── reservationId: string (PK)
├── customerId: string (FK)
├── slotId: string (FK)
├── vehiclePlate: string
├── vehicleType: enum
├── reservationDate: datetime
├── expectedDuration: integer (minutes)
├── estimatedFee: decimal
├── status: enum (pending, confirmed, checked_in, checked_out, cancelled)
├── notes: string
├── createdAt: datetime
├── updatedAt: datetime
```

### Payment
```
Payment
├── paymentId: string (PK)
├── ticketId: string (FK)
├── customerId: string (FK)
├── amount: decimal
├── method: enum (cash, card, transfer)
├── transactionId: string
├── status: enum (pending, success, failed, refunded)
├── paidAt: datetime
├── createdAt: datetime
```

### Pricing
```
Pricing
├── pricingId: string (PK)
├── vehicleType: enum
├── pricePerHour: decimal
├── pricePerDay: decimal
├── minFee: decimal
├── maxFee: decimal
├── effectiveDate: datetime
├── endDate: datetime
├── isActive: boolean
├── createdAt: datetime
├── updatedAt: datetime
```

---

# 🗺️ API ENDPOINTS MAP

## Customer Endpoints (22)

### Auth
```
POST   /api/auth/register/customer
POST   /api/auth/verify-otp
POST   /api/auth/login
POST   /api/auth/change-password
POST   /api/auth/forgot-password
```

### Account
```
GET    /api/auth/me
PUT    /api/customers/{customerId}
```

### Reservations
```
GET    /api/reservations
GET    /api/reservations/{reservationId}
POST   /api/reservations
DELETE /api/reservations/{reservationId}
GET    /api/reservations/available-slots
```

### Tickets
```
GET    /api/tickets
GET    /api/tickets/{ticketId}
GET    /api/tickets/{ticketId}/payment-history
```

### Monthly Tickets
```
GET    /api/monthly-tickets/customer/{customerId}
GET    /api/monthly-tickets/{monthlyTicketId}
GET    /api/pricing/monthly
POST   /api/monthly-tickets
POST   /api/monthly-tickets/{monthlyTicketId}/renew
```

## Manager Endpoints (40+)

### Auth & Account
```
POST   /api/auth/login
GET    /api/auth/me
PUT    /api/managers/{managerId}
GET    /api/managers/{managerId}/activity-log
POST   /api/auth/change-password
```

### Tickets
```
GET    /api/tickets
GET    /api/tickets/search
GET    /api/pricing
PUT    /api/pricing/{pricingId}
```

### Employees
```
GET    /api/employees
GET    /api/employees/{employeeId}/detail
GET    /api/employees/search
POST   /api/employees/invite
PUT    /api/employees/{employeeId}
DELETE /api/employees/{employeeId}
```

### Parking Slots
```
GET    /api/parking-slots
GET    /api/parking-slots/{slotId}
PATCH  /api/parking-slots/{slotId}/status
GET    /api/parking-slots/summary
GET    /api/parking-slots/report
```

### Reports
```
GET    /api/reports/manager/dashboard
POST   /api/reports/manager/revenue
GET    /api/reports/manager/customers
GET    /api/reports/manager/comprehensive
```

## Employee Endpoints (25+)

### Auth & Account
```
POST   /api/auth/login
GET    /api/auth/me
PUT    /api/employees/{employeeId}
POST   /api/auth/change-password
```

### Customers
```
GET    /api/employees/customers
GET    /api/employees/customers/search
```

### Tickets
```
GET    /api/employees/tickets
GET    /api/employees/tickets/search
POST   /api/tickets
POST   /api/tickets/checkin/confirm
POST   /api/tickets/checkout/confirm
```

### Parking Slots
```
GET    /api/employees/parking-slots
GET    /api/employees/parking-slots/{slotId}
```

### Reports
```
GET    /api/reports/employee/{employeeId}/dashboard
GET    /api/reports/employee/{employeeId}/attendance
GET    /api/reports/employee/{employeeId}/revenue
```

---

# 🗄️ DATABASE SCHEMA RELATIONSHIPS

```
Account (1) ──→ (1) Customer
Account (1) ──→ (1) Manager
Account (1) ──→ (1) Employee

Customer (1) ──→ (N) Ticket
Customer (1) ──→ (N) Reservation
Customer (1) ──→ (N) MonthlyTicket
Customer (1) ──→ (N) Payment

ParkingSlot (1) ──→ (N) Ticket
ParkingSlot (1) ──→ (N) Reservation

Ticket (1) ──→ (N) Payment

Reservation (0..1) → (1) ParkingSlot

Employee (1) ──→ (N) ShiftRecord
Employee (1) ──→ (N) ActivityLog
```

---

## 🎯 SUMMARY

**Total Endpoints:** 87+
**Customer Endpoints:** 22
**Manager Endpoints:** 40+
**Employee Endpoints:** 25+

**Main Features:**
- ✅ Authentication (Register, Login, OTP)
- ✅ Account Management
- ✅ Reservations
- ✅ Ticket Processing
- ✅ Monthly Subscriptions
- ✅ Employee Management
- ✅ Parking Slot Management
- ✅ Reporting & Analytics
- ✅ Payment Processing

---

**Status:** Ready for Implementation  
**Next Step:** Start with DTOs and service methods  
**Estimated Timeline:** 2-3 days
