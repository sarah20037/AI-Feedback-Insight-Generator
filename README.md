# AI Feedback Analyzer 

AI Feedback Analyzer is a simple webpage that lets customers submit text reviews and automatically uses AI to analyze them. 

The AI scans the review text to generate:
- **Sentiment** (Positive, Negative, or Neutral)
- **Summary** (A short summary of the review)
- **Category** (e.g. Speed, Design, Bug, etc.)
- **Recommended Action** (Suggested fix)

The analyzed feedback is saved to a SQL database and shown on an Admin Dashboard screen using nice pie charts.

---

## Simple Documentation
Detailed guides are in the `docs/` folder:
- [Architecture Overview](./docs/architecture_overview.md) — How the database, server, and website connect.
- [API Contracts](./docs/api_contracts.md) — The parameters the server sends and receives.
- [Data Flow / Sequence Diagrams](./docs/data_flow.md) — Step-by-step maps of how data moves.
- [Assumptions Made](./docs/assumptions.md) — Things we assumed during development.

---

## Tech Stack Used

### Frontend (Website UI)
- **Angular** (TypeScript, HTML, CSS)

### Backend (Web Server)
- **ASP.NET Core** (C#)

### Database
- **Microsoft SQL Server** (using Stored Procedures)

### AI Model
- **OpenRouter API** (`meta-llama/llama-3-8b-instruct`)

---

## Setup Instructions

### 1. Install Frontend Dependencies
Open your command terminal, navigate to the `frontend` folder, and install package dependencies:
```powershell
cd frontend
npm install
```

### 2. Configure Database & AI Keys
Open the backend settings file:
`backend/FeedbackAPI/appsettings.json`

Add your SQL database connection string and your OpenRouter API Key:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AIFeedbackDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "OpenRouter": {
    "ApiKey": "YOUR_OPENROUTER_API_KEY",
    "BaseUrl": "https://openrouter.ai/api/v1/chat/completions",
    "Model": "meta-llama/llama-3-8b-instruct"
  }
}
```

### 3. Check Database
Make sure:
- Microsoft SQL Server is running.
- You created a database named `AIFeedbackDB`.
- You ran the SQL scripts from the `Stored Procedures` folder in your database.

---

## How to Run API

1. Open a command terminal.
2. Go to the backend directory:
   ```powershell
   cd backend/FeedbackAPI
   ```
3. Run the server:
   ```powershell
   dotnet run
   ```
   - Server runs at: `http://localhost:5048`
   - View API Swagger documentation: `http://localhost:5048/swagger`

---

## How to Run UI

1. Open a new command terminal.
2. Go to the frontend directory:
   ```powershell
   cd frontend
   ```
3. Start the website:
   ```powershell
   npm start
   ```
   - Open your browser to: `http://localhost:4200`

---

## How to Run Tests

### Run Backend Server Tests
To run the server unit tests:
1. Open a terminal.
2. Go to the test directory:
   ```powershell
   cd backend/FeedbackAPI.Tests
   ```
3. Run test command:
   ```powershell
   dotnet test
   ```

### Run Frontend Website Tests
To run the browser unit tests:
1. Open a terminal.
2. Go to the frontend directory:
   ```powershell
   cd frontend
   ```
3. Run test command:
   ```powershell
   npm test
   ```
## Screenshots

### Screenshot 1
![Screenshot 1](./screenshots/1.png)

<br>

### Screenshot 2
![Screenshot 2](./screenshots/2.png)

<br>

### Screenshot 3
![Screenshot 3](./screenshots/3.png)

<br>

### Screenshot 4
![Screenshot 4](./screenshots/4.png)

---
---
