# VoC Analysis System

An internal AI-powered Voice of Customer platform that automatically collects, analyzes, and summarizes customer feedback — giving management real-time intelligence without manual effort.

---

## What It Does

Companies receive customer feedback across many platforms simultaneously. Reading and making sense of this manually is slow, inconsistent, and impossible to scale. This system solves that by ingesting feedback from multiple sources, running AI analysis automatically in the background, and presenting clear insights and charts to the management team.

Customers never interact with this system directly. It works silently in the background, processing feedback that customers have already posted elsewhere.

---

## Key Features

- Automatic sentiment analysis using a locally running AI model — no API key required
- Topic detection that groups feedback into categories such as Delivery Speed, Product Quality, and Customer Support
- AI-generated management summaries with urgency levels for each topic
- Interactive dashboard with sentiment distribution charts, topic breakdown charts, and performance comparison across time periods
- Period filter allowing managers to view data for the last 7 days, 30 days, 90 days, or all time
- CSV bulk upload for importing large volumes of feedback at once
- Manual feedback entry for individual records
- Automated urgent email alerts when critical negative feedback is detected
- Daily AI-generated reports summarizing each day's feedback
- Role-based access control separating Analyst and Executive views
- JWT authentication with automatic session expiry handling

---

## Screenshots

### Executive Dashboard
The home dashboard shows KPI cards, sentiment distribution, feedback by topic, and performance trends across time periods.

### Feedback Management
The analyst view shows all feedback in a paginated table with filters for sentiment, source, and keyword search. Supports CSV upload and manual entry.

### Topics and Insights
AI-generated insights grouped by urgency level with a sticky navigation panel and feedback count indicators.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 17 with TypeScript |
| Backend | ASP.NET Core 8 with C# |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| AI Engine | Ollama with llama3.2 running locally |
| Authentication | JWT with role-based authorization |
| Charts | Chart.js |

---

## Project Structure

```
VOC/
    api/
        HotelVoC.API/
            Controllers/        REST API endpoints
            Workers/            Background AI analysis worker
            Middleware/         Global error handling
            DTOs/               Data transfer objects
        HotelVoC.Core/
            Models/             Database entity classes
            Services/           Business logic
            Interfaces/         Service contracts
            Data/               Entity Framework DbContext
    frontend/
        HotelVoC.Web/
            src/app/
                core/           Services, guards, interceptors
                pages/          Login, Dashboard, Feedback, Insights
                shared/         Navbar, Sidebar, Toast notifications
```

---

## User Roles

| Role | Access |
|---|---|
| Analyst | Feedback Management page, Topics and Insights page, CSV upload, manual feedback entry |
| Executive | Home Dashboard with all charts, Topics and Insights page, period filtering, CSV export |

---

## How the AI Works

The system uses Ollama to run the llama3.2 model entirely on your local machine. No data is sent to any external server and no API key is required.

A background worker runs every 3 minutes and picks up any unanalyzed feedback. It sends each feedback record to the local AI model and receives back a sentiment label and a topic name. If the feedback is negative and contains keywords related to delivery problems, refunds, or fraud, an email alert is automatically sent to the configured manager address.

Insight generation is triggered manually from the Topics and Insights page. The AI reads all feedback grouped by topic and writes a 2-sentence management summary with an urgency level.

---

## Quick Setup

Full setup instructions including all SQL queries, configuration steps, and troubleshooting are in PROJECT.md.

The short version:

1. Install .NET 8, Node.js 18, SQL Server, and Ollama
2. Run the SQL setup script from PROJECT.md to create the database and tables
3. Pull the AI model by running: ollama pull llama3.2
4. Update the connection string in appsettings.json
5. Run the backend with: dotnet run
6. Run the frontend with: ng serve
7. Open http://localhost:4200 and log in

Default analyst login: analyst@company.com with password: password

Default manager login: manager@company.com with password: password

---

## Setup Documentation

For complete setup instructions, database queries, configuration details, API reference, and troubleshooting, see PROJECT.md.

---

## Notes

- The AI runs entirely on your local machine and requires no internet connection after the initial model download
- The background worker processes up to 10 feedback records per cycle
- JWT tokens expire after 8 hours and the user must log in again
- All passwords are stored as bcrypt hashes and never as plain text
