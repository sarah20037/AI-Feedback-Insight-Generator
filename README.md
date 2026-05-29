# AI Feedback Analyzer

## Project Overview

AI Feedback Analyzer is a full-stack web application that allows customers to submit feedback and receive AI-powered analysis. The application sends feedback to an AI model through the OpenRouter API and generates:

* Sentiment (Positive, Neutral, Negative)
* Summary
* Category
* Recommended Action

The analyzed feedback is stored in SQL Server and displayed through an admin dashboard.

### Customer Features

* Register and Login
* Submit Feedback
* View AI Analysis Results

### Admin Features

* View All Feedback
* Sentiment Statistics
* Pie Chart Visualization
* Top Negative Feedback
* Latest Feedback Records

---

## Tech Stack Used

### Frontend

* Angular 21
* TypeScript
* HTML
* CSS

### Backend

* ASP.NET Core Web API (.NET 10)
* C#

### Database

* SQL Server
* Stored Procedures

### AI Integration

* OpenRouter Chat Completions API
* Meta Llama 3 8B Instruct

### Data Access

* Microsoft.Data.SqlClient

---

## Setup Instructions

### 1. Install Dependencies

Frontend:

```powershell
cd frontend
npm install
```

### 2. Configure Database

Update the SQL Server connection string in:

```text
backend/FeedbackAPI/appsettings.json
```

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "YOUR_CONNECTION_STRING"
}
```

### 3. Configure OpenRouter API

In `backend/FeedbackAPI/appsettings.json`:

```json
"OpenRouter": {
  "ApiKey": "YOUR_API_KEY",
  "BaseUrl": "https://openrouter.ai/api/v1/chat/completions",
  "Model": "meta-llama/llama-3-8b-instruct"
}
```

### 4. Database Requirements

Ensure:

* SQL Server is running
* Database is created
* Required stored procedures are available

---

## How to Run API

Navigate to the backend project:

```powershell
cd backend/FeedbackAPI
```

Run the API:

```powershell
dotnet run
```

API URL:

```text
http://localhost:5048
```

Swagger URL:

```text
http://localhost:5048/swagger
```

---

## How to Run UI

Navigate to the frontend folder:

```powershell
cd frontend
```

Start the Angular application:

```powershell
npm start
```

UI URL:

```text
http://localhost:4200
```

Configured API Endpoint:

```text
http://127.0.0.1:5048/api
```

---

## How to Run Tests

### Frontend Tests

```powershell
cd frontend
npm test
```

### Frontend Build Verification

```powershell
cd frontend
npm run build
```

### Backend Build Verification

```powershell
cd backend/FeedbackAPI
dotnet build
```

Currently, there is no separate backend test project. The backend can be verified by successfully building the project and running the API.
