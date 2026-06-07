# VoC Analysis System

## Overview

The VoC (Voice of Customer) Analysis System is an internal web application that collects customer feedback from multiple channels, applies AI-driven analysis, and presents actionable insights to company management.

This is not a customer-facing product. Customers never interact with it directly. The system ingests feedback that customers have already posted on external platforms and surfaces intelligence for internal teams.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 17 (TypeScript) |
| Backend | ASP.NET Core 8 (C#) |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| AI Engine | Ollama (llama3.2 - runs locally) |
| Authentication | JWT (JSON Web Token) |

---

## Project Structure

```
VOC/
    api/
        VOCDb.API/        - ASP.NET Core Web API (controllers, workers, middleware)
        VOCDb.Core/       - Business logic (services, models, interfaces, data)
    frontend/
        VOCDb.Web/        - Angular SPA (pages, services, components)
    VOCDb.slnx            - Solution file
```

---

## Prerequisites

Make sure the following are installed on your system before running the project.

### Required Software

| Software | Version | Download |
|---|---|---|
| .NET SDK | 8.0 or higher | https://dotnet.microsoft.com/download |
| Node.js | 18.0 or higher | https://nodejs.org |
| Angular CLI | 17.x | npm install -g @angular/cli@17 |
| SQL Server | 2019 or higher | https://www.microsoft.com/sql-server |
| SQL Server Management Studio | Any | https://aka.ms/ssmsfullsetup |
| Ollama | Latest | https://ollama.com |

---

## Step 1 - Database Setup

Open SQL Server Management Studio and run the following SQL queries in order.

### 1.1 Create the Database

```sql
CREATE DATABASE VOCDb;
USE VOCDb;
```

### 1.2 Create All Tables

```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(512) NOT NULL,
    Role VARCHAR(30) NOT NULL
);

CREATE TABLE FeedbackSources (
    SourceId INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Feedbacks (
    FeedbackId INT PRIMARY KEY IDENTITY,
    SourceId INT FOREIGN KEY REFERENCES FeedbackSources(SourceId),
    CustomerIdentifier NVARCHAR(256),
    RawText NVARCHAR(MAX) NOT NULL,
    SubmittedAt DATETIME2 NOT NULL,
    IsAnalyzed BIT DEFAULT 0
);

CREATE TABLE SentimentResults (
    SentimentId INT PRIMARY KEY IDENTITY,
    FeedbackId INT FOREIGN KEY REFERENCES Feedbacks(FeedbackId),
    Label VARCHAR(20) NOT NULL
);

CREATE TABLE Topics (
    TopicId INT PRIMARY KEY IDENTITY,
    FeedbackId INT FOREIGN KEY REFERENCES Feedbacks(FeedbackId),
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Insights (
    InsightId INT PRIMARY KEY IDENTITY,
    Title NVARCHAR(200),
    Summary NVARCHAR(MAX),
    TopicName NVARCHAR(100),
    UrgencyLevel VARCHAR(20),
    FeedbackCount INT
);

CREATE TABLE DailyReports (
    ReportId INT PRIMARY KEY IDENTITY,
    ReportDate DATE NOT NULL,
    TotalFeedback INT,
    PositiveCount INT,
    NegativeCount INT,
    NeutralCount INT,
    TopIssue NVARCHAR(100),
    Summary NVARCHAR(MAX),
    GeneratedAt DATETIME2 DEFAULT GETDATE()
);
```

### 1.3 Seed Initial Data

```sql
-- Feedback Sources
INSERT INTO FeedbackSources (Name) VALUES
('Amazon Reviews'),
('Trustpilot'),
('Customer Survey');

-- Default Users
-- Password for both accounts is: password
INSERT INTO Users (Email, PasswordHash, Role) VALUES
('analyst@company.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.', 'Analyst'),
('manager@company.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.', 'Executive');
```

### 1.4 Optional - Sample Feedback Data

Run this to populate the system with sample data for testing.

```sql
INSERT INTO Feedbacks (SourceId, CustomerIdentifier, RawText, SubmittedAt, IsAnalyzed) VALUES
(1, 'customer_001', 'Package arrived 3 weeks late and was completely damaged.', CAST(DATEADD(DAY,-2,GETDATE()) AS DATE), 0),
(1, 'customer_002', 'Excellent product quality and very fast delivery loved it.', CAST(DATEADD(DAY,-2,GETDATE()) AS DATE), 0),
(1, 'customer_003', 'Seller sent wrong item. Return process was a nightmare.', CAST(DATEADD(DAY,-2,GETDATE()) AS DATE), 0),
(1, 'customer_004', 'Great value for the price. Will definitely order again.', CAST(DATEADD(DAY,-3,GETDATE()) AS DATE), 0),
(1, 'customer_005', 'Item stopped working after 3 days. Very poor quality.', CAST(DATEADD(DAY,-3,GETDATE()) AS DATE), 0),
(2, 'customer_006', 'Customer support was unhelpful and kept closing my ticket.', CAST(DATEADD(DAY,-5,GETDATE()) AS DATE), 0),
(2, 'customer_007', 'Perfect product, fast shipping, great communication from seller.', CAST(DATEADD(DAY,-5,GETDATE()) AS DATE), 0),
(2, 'customer_008', 'Refund took over 3 weeks. Terrible experience overall.', CAST(DATEADD(DAY,-7,GETDATE()) AS DATE), 0),
(2, 'customer_009', 'Product quality is outstanding. Highly recommend this seller.', CAST(DATEADD(DAY,-7,GETDATE()) AS DATE), 0),
(3, 'customer_010', 'App kept crashing during checkout. Lost my cart multiple times.', CAST(DATEADD(DAY,-10,GETDATE()) AS DATE), 0);
```

---

## Step 2 - AI Engine Setup (Ollama)

The system uses Ollama to run AI locally on your machine. No API key or internet connection is required after initial setup.

### 2.1 Install Ollama

Download and install Ollama from https://ollama.com

### 2.2 Pull the AI Model

Open a terminal and run the following command. This downloads the llama3.2 model (approximately 2GB).

```
ollama pull llama3.2
```

### 2.3 Verify Ollama is Running

Open your browser and go to:

```
http://localhost:11434
```

You should see the message: Ollama is running

---

## Step 3 - Backend Setup

### 3.1 Navigate to the API Project

```
cd VOC/api/VOCDb.API
```

### 3.2 Install NuGet Packages

```
dotnet restore
```

### 3.3 Configure Connection String

Open the file `VOCDb.API/appsettings.json` and update the connection string to match your SQL Server instance.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=VOCDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-minimum-32-characters-long",
    "Issuer": "VOCDbApp",
    "Audience": "VOCDbUsers"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama3.2"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-gmail@gmail.com",
    "SenderPassword": "your-gmail-app-password",
    "ManagerEmail": "manager-email@gmail.com"
  }
}
```

To find your SQL Server name, open SQL Server Management Studio and check the server name shown in the connection window. Common formats are:

- localhost
- DESKTOP-XXXXX\\SQLEXPRESS
- .\\SQLEXPRESS

### 3.4 Configure Gmail App Password (for Email Alerts)

1. Go to myaccount.google.com
2. Click Security
3. Enable 2-Step Verification
4. Search for App Passwords
5. Create a new app password and copy the 16-character code
6. Paste it into the SenderPassword field in appsettings.json

### 3.5 Run the Backend

```
dotnet run
```

The API will start at http://localhost:5157

Swagger documentation is available at http://localhost:5157/swagger

---

## Step 4 - Frontend Setup

### 4.1 Navigate to the Angular Project

```
cd VOC/frontend/VOCDb.Web
```

### 4.2 Install Node Packages

```
npm install --legacy-peer-deps
```

### 4.3 Install Additional Dependencies

```
npm install chart.js --legacy-peer-deps
npm install @swimlane/ngx-charts@20.5.0 --legacy-peer-deps
```

### 4.4 Run the Frontend

```
ng serve
```

The application will open at http://localhost:4200

---

## Step 5 - Login

Open your browser and go to http://localhost:4200

Use the following default credentials.

| Role | Email | Password |
|---|---|---|
| Analyst | analyst@company.com | password |
| Executive | manager@company.com | password |

The Analyst role has access to the Feedback Management page and Topics and Insights page.

The Executive role has access to the Home dashboard, Topics and Insights page, and all analytics charts.

To create additional users, insert records directly into the Users table in the database using a bcrypt-hashed password.

---

## How the System Works

### Feedback Ingestion

The Analyst logs in and uploads a CSV file containing customer feedback. The CSV must follow this format with exactly four columns:

```
SourceId, CustomerIdentifier, RawText, SubmittedAt
1, customer_001, Package arrived late and damaged, 2026-05-01
2, customer_002, Great product fast delivery, 2026-05-02
```

The analyst can also add individual feedback records manually using the Add Feedback button.

### AI Analysis (Automatic)

A background worker runs automatically every 3 minutes. It picks up all feedback records where IsAnalyzed equals 0, sends each one to Ollama, receives a sentiment label and topic name, saves the results to the database, and marks the record as analyzed.

No manual action is required for this step.

### Urgent Email Alerts

If the AI detects a negative feedback containing keywords related to delivery, refund, damage, missing items, fraud, or scams, an email alert is automatically sent to the configured manager email address.

### Insight Generation

On the Topics and Insights page, clicking Generate Insights triggers the AI to read all analyzed feedback grouped by topic and write a 2-sentence management summary with an urgency level for each topic. Urgency levels are Low, Medium, High, or Critical based on the volume and percentage of negative feedback.

### Executive Dashboard

The dashboard displays KPI cards, sentiment distribution charts, feedback by topic charts, performance comparison across time periods, and AI-generated daily reports. The manager can filter all data by the last 7 days, 30 days, 90 days, or all time.

---

## CSV Upload Format

When uploading feedback via CSV, the file must have exactly four columns in this order with no header row required (the system skips the first row automatically).

| Column | Description | Example |
|---|---|---|
| SourceId | 1 = Amazon Reviews, 2 = Trustpilot, 3 = Customer Survey | 1 |
| CustomerIdentifier | Customer ID or email or anonymous | customer_042 |
| RawText | The feedback text | Package arrived late |
| SubmittedAt | Date in YYYY-MM-DD format | 2026-05-01 |

---

## API Endpoints

All endpoints except the login endpoint require a valid JWT token in the Authorization header.

| Method | Endpoint | Description |
|---|---|---|
| POST | /api/auth/login | Login and receive JWT token |
| POST | /api/feedback/ingest | Add a single feedback record |
| POST | /api/feedback/bulk-ingest | Upload a CSV file |
| GET | /api/feedback | Get all feedback with filters |
| GET | /api/feedback/{id} | Get a single feedback by ID |
| GET | /api/analytics/sentiment-summary | Get sentiment breakdown |
| GET | /api/analytics/topics | Get topic breakdown |
| GET | /api/analytics/comparison | Get time period comparison |
| GET | /api/analytics/daily-reports | Get AI daily reports |
| GET | /api/dashboard/stats | Get dashboard KPI numbers |
| GET | /api/insights | Get all AI insights |
| POST | /api/insights/generate | Trigger insight generation |

---

## Troubleshooting

### Cannot connect to database

Check that SQL Server is running. Open SQL Server Management Studio, connect to your server, and verify the VOCDb database exists. Update the connection string in appsettings.json with your exact server name.

### Invalid email or password on login

The default password hash in the seed data corresponds to the word password. If login fails, run the test-hash endpoint in Swagger to generate a fresh hash, then update both user records in the database with the new hash.

### Feedback not being analyzed

Make sure Ollama is running by visiting http://localhost:11434 in your browser. If Ollama is not running, open a terminal and run the command: ollama run llama3.2

The background worker runs every 3 minutes. Wait a few minutes and refresh the feedback list to see updated analysis results.

### Email alerts not sending

Make sure you have created a Gmail App Password (not your regular Gmail password). Enable 2-Step Verification in your Google Account, then generate an App Password and use that 16-character code in appsettings.json.

### Angular build errors

Run the following commands to reinstall dependencies cleanly.

```
npm cache clean --force
npm install --legacy-peer-deps
ng serve
```

---

## Default User Roles

| Role | Access |
|---|---|
| Analyst | Feedback Management, Topics and Insights |
| Executive | Home Dashboard, Topics and Insights, All Analytics |

To add a new user, insert a record into the Users table in the database. Use a bcrypt hash generator to hash the password before inserting.

---

## Notes

- The AI engine runs entirely on your local machine. No data is sent to external servers.
- The background worker processes up to 10 feedback records per cycle.
- If Ollama fails to analyze a feedback after 3 attempts, it is marked as Unclassified and will not be retried.
- JWT tokens expire after 8 hours. The user must log in again after expiry.
- Daily reports are generated automatically by the background worker once per day based on that day's analyzed feedback.