# VoC Analysis System — Project Plan 

**This is NOT a feedback form for customers.**
Customers never open this app.

This is an **internal tool for the company's management team.**
It collects feedback that customers already posted elsewhere,
runs AI on it, and shows the results to managers.

```
Customers write reviews/tickets         Managers use THIS app
─────────────────────────────           ──────────────────────
App Store reviews        ──→
Zendesk support tickets  ──→   VoC App  ──→  Dashboard + AI Insights
CSV exports from surveys ──→
```

### Who logs into this app?

| Role | What they do |
|------|-------------|
| Analyst | Uploads CSVs, checks feedback is coming in |
| Executive/Manager | Views dashboard, reads AI insights |

---

## How the system works — step by step

```
Step 1: Analyst uploads a CSV file (or feedback comes via API source)

Step 2: Feedback saved to database  →  IsAnalyzed = 0

Step 3: Background worker runs automatically every few minutes
        → Picks up unanalyzed feedback
        → Calls OpenAI

Step 4: AI returns:  Sentiment label  +  Topic name

Step 5: Results saved to database

Step 6: Executive opens Dashboard → sees charts, trends, AI insights

```

## Pages 

| Page | Who uses it | What it shows |
|------|------------|--------------|
| Login | Everyone | Email + password form | 
| Dashboard | Executive | KPI cards, sentiment chart, top topics, AI insights |
| Feedback List | Analyst | Table of all feedback + Upload CSV button |
| Topics & Insights | Both | Topic list + AI summary for each topic |

> Sources management is **not a page** — sources are pre-loaded in the database.
> No need to build a UI for it.

---

## APIs — 9 total (removed what is not needed)

### Auth
| # | Endpoint | Does what |
|---|----------|-----------|
| 1 | POST /api/auth/login | Login, returns JWT token |

### Feedback
| # | Endpoint | Does what |
|---|----------|-----------|
| 2 | POST /api/feedback/ingest | Analyst manually enters one feedback |
| 3 | POST /api/feedback/bulk-ingest | Analyst uploads a CSV file |
| 4 | GET /api/feedback | List all feedback with filters |
| 5 | GET /api/feedback/{id} | View one feedback in detail |

### Analytics
| # | Endpoint | Does what |
|---|----------|-----------|
| 6 | GET /api/analytics/sentiment-summary | Returns % positive / negative / neutral |
| 7 | GET /api/analytics/topics | Returns list of topic clusters |
| 8 | GET /api/analytics/insights | Returns AI-written insight summaries |

### Dashboard
| # | Endpoint | Does what |
|---|----------|-----------|
| 9 | GET /api/dashboard/stats | Returns all numbers for the dashboard page |

> AI analysis runs automatically via a background worker — no API endpoint needed for it.
> Token refresh removed — basic login is enough for this project.

---

## Database — 5 tables (simplified)

### Feedbacks
Every piece of collected feedback goes here.

| Column | Type | Note |
|--------|------|------|
| FeedbackId | INT PK | Auto increment |
| SourceId | INT FK | Which channel it came from |
| CustomerIdentifier | NVARCHAR(256) | Email or anonymous ID |
| RawText | NVARCHAR(MAX) | The actual feedback text |
| SubmittedAt | DATETIME2 | When customer originally wrote it |
| IsAnalyzed | BIT | 0 = waiting for AI, 1 = done |

### SentimentResults
AI sentiment result for each feedback.

| Column | Type | Note |
|--------|------|------|
| SentimentId | INT PK | Auto increment |
| FeedbackId | INT FK | Links to Feedbacks |
| Label | VARCHAR(20) | Positive / Negative / Neutral |

### Topics
AI-generated topic groups.

| Column | Type | Note |
|--------|------|------|
| TopicId | INT PK | Auto increment |
| Name | NVARCHAR(100) | e.g. "Delivery Speed", "Billing Issues" |
| FeedbackId | INT FK | Which feedback this topic belongs to |

### Insights
AI-written summary shown to management.

| Column | Type | Note |
|--------|------|------|
| InsightId | INT PK | Auto increment |
| Title | NVARCHAR(200) | One-line headline |
| Summary | NVARCHAR(MAX) | AI-written 2-sentence paragraph |
| TopicName | NVARCHAR(100) | Which topic this covers |
| UrgencyLevel | VARCHAR(20) | Low / Medium / High / Critical |
| FeedbackCount | INT | How many feedbacks support this |

### Users
People who log into this system.

| Column | Type | Note |
|--------|------|------|
| UserId | INT PK | Auto increment |
| Email | NVARCHAR(256) | Login email |
| PasswordHash | NVARCHAR(512) | Bcrypt hashed password |
| Role | VARCHAR(30) | Analyst / Executive |

> FeedbackSources table removed from code — just seed 2-3 source names directly in DB.
> FeedbackTopics junction table removed — each feedback gets one topic, kept simple.

---

## Where AI runs

AI runs only in the backend via a background worker. Angular never calls AI.

| What | AI does | When |
|------|---------|------|
| Sentiment | Labels feedback Positive / Negative / Neutral | Auto — background worker picks up IsAnalyzed=0 |
| Topic | Assigns a topic name to the feedback | Same call, same time as sentiment |
| Insight summary | Writes a 2-sentence summary per topic | When analyst clicks "Generate Insights" |

### Prompt examples for u to refer:

**Sentiment + Topic (one call):**
```
System: You are a feedback analyser. Reply in JSON only.
User:   Analyse this customer feedback.
        Return: {
          "sentiment": "Positive|Negative|Neutral",
          "topic": "one short topic name"
        }
        Feedback: "Package arrived 5 days late and the box was damaged."
```

**Insight prompt:**
```
System: You are writing short business insights for management.
User:   These 38 feedbacks are about "Delivery Speed".
        Write a 2-sentence insight and give an urgency level.
        Return: { "title": "...", "summary": "...", "urgency": "High" }
```

---

## Things to keep in mind

- Customers NEVER use this app — internal only
- AI runs in the background automatically — do not call AI from Angular
- Controllers only handle routing — all logic goes in Services
- Never return EF Entity classes from API — always use DTOs
- Never hardcode the OpenAI API key — store it in appsettings.json
- Use Swagger to test every API before connecting Angular

