# API Contracts (Simple Guide)

This guide shows how the frontend (webpage) talks to the backend (server). All information is sent as standard JSON.

---

## 1. Authentication (Login and Register)

### Register a New Account
- **URL**: `POST /api/auth/register`
- **What to send (Request)**:
  ```json
  {
    "fullName": "Sarah Smith",
    "email": "sarah@example.com",
    "username": "sarahsmith",
    "passwordHash": "password123"
  }
  ```
- **What you get back if successful (Response)**:
  ```json
  {
    "message": "User Registered Successfully",
    "customerId": 1,
    "username": "sarah@example.com",
    "fullName": "Sarah Smith",
    "role": "customer"
  }
  ```
- **Error**: If the email is already taken, you get a `409 Conflict` error message.

### Customer Login
- **URL**: `POST /api/auth/login`
- **What to send (Request)**:
  ```json
  {
    "username": "sarahsmith",
    "passwordHash": "password123"
  }
  ```
- **What you get back if successful (Response)**:
  ```json
  {
    "message": "Login Successful",
    "customerId": 1,
    "username": "sarahsmith",
    "fullName": "Sarah Smith",
    "role": "customer"
  }
  ```

### Admin Login
- **URL**: `POST /api/auth/admin-login`
- **What to send (Request)**:
  ```json
  {
    "username": "admin@example.com",
    "passwordHash": "adminpassword"
  }
  ```
- **What you get back if successful (Response)**:
  ```json
  {
    "message": "Admin Login Successful",
    "username": "Admin",
    "role": "admin"
  }
  ```

---

## 2. Feedback Reviews

### Submit a Review
- **URL**: `POST /api/feedback/submit`
- **What to send (Request)**:
  ```json
  {
    "customerId": 1,
    "feedbackText": "The login page is slow."
  }
  ```
- **What you get back if successful (Response)**:
  ```json
  {
    "message": "Feedback Submitted Successfully",
    "feedback": {
      "feedbackId": 12,
      "customerId": 1,
      "feedbackText": "The login page is slow.",
      "summary": "Login page is slow.",
      "sentiment": "NEGATIVE",
      "issueCategory": "Performance",
      "recommendedAction": "Optimize loading.",
      "createdAt": "2026-06-17T07:54:00"
    }
  }
  ```

### Get All Reviews (For Admin)
- **URL**: `GET /api/feedback`
- **What you get back (Response)**:
  - Returns a list containing all the feedback reviews saved in the database.

### Get Dashboard Overview Stats (For Admin)
- **URL**: `GET /api/feedback/overview`
- **What you get back (Response)**:
  - Returns positive, negative, and neutral review counts, along with the latest negative items to construct dashboard charts.

### Get Reviews page-by-page (Pagination)
- **URL**: `GET /api/feedback/page?page=1&pageSize=10`
- **What you get back (Response)**:
  - Returns a list of 10 feedback items for the requested page number.

### Get Past Feedback for a Specific Customer
- **URL**: `GET /api/feedback/customer/{customerId}`
- **What you get back (Response)**:
  - Returns a list of past reviews submitted by that specific customer.
