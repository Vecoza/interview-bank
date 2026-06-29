# Interview Question Bank

> An interview prep tool built by someone who got tired of losing flashcards and making excuses.
> Now the excuses are automated.

**Stack:** ASP.NET Core 10 · Angular 18 · PostgreSQL · JWT Auth · Claude AI  
**Hosting:** Fly.io (API) · Vercel (UI) · GitHub Actions (CI/CD) — all free tier

---

## Live

| Service | URL |
|---------|-----|
| Angular SPA | `https://interview-bank.vercel.app` |
| .NET API | `https://interview-bank-api.fly.dev` |
| API docs (Scalar) | `https://interview-bank-api.fly.dev/scalar` |

---

## What does this thing actually do?

Glad you asked, imaginary reader.

### Your Question Bank
Add interview questions. Assign topics and difficulty. Write the model answer so Future You — the one sweating through a technical screen at 9am — can actually study something useful. Filter by topic, difficulty, or "questions I've never looked at" (the largest category, statistically speaking).

### Mock Interview Mode
Sit down. Start a session. A countdown timer starts. You can no longer pretend you're "almost ready". You have to answer or you don't. Three self-assessment buttons judge you afterward: **Got it**, **Partially**, and **Missed it** (the one you'll press more than you'd like to admit).

**Two question types, because the world is binary:**

#### Essay questions
You write your answer. The clock runs. You hit "Reveal answer" when you either know it or give up. Then you can hit **"Check my answer with AI"** and Claude will evaluate your response against the model answer, score it 0–100, and tell you honestly where you went wrong. Unlike your friends, it won't soften the blow.

#### Yes / No questions
Sometimes the answer is just yes or no. Is `null == undefined` in JavaScript? Can SQL do a self-join? You get two big buttons. You pick one. The app tells you immediately whether you're right or tragically wrong. No self-assessment needed — the machine has judged you already.

### Spaced Repetition
The app tracks when you last practiced each question and schedules the next review using spaced repetition. Questions you got wrong come back sooner. Questions you nailed go on holiday for a while. The algorithm doesn't care about your feelings.

### Question Library
A curated set of JavaScript, C#, SQL, and Algorithms questions — ready to use without setup. Browse them, cherry-pick the ones you want, and import them into your bank. Or use them directly in a mock interview without importing at all (the lazy-but-effective option).

### Progress Dashboard
Charts showing your practice history by topic and difficulty. A streak counter. Your weakest topics highlighted. It's the "oh no" dashboard. You'll check it once a week, feel bad, then practice for 20 minutes and feel better.

---

## Features

- JWT auth with HttpOnly refresh-token rotation (no localStorage tokens, we're not animals)
- Full question CRUD — URL-synced filters, pagination, practiced-state tracking
- **Yes/No questions** — instant grading during mock interviews, no second-guessing
- **AI essay evaluation** — Claude scores your answer 0–100 and gives specific feedback
- **Mock Interview from Library** — use curated questions directly, no import required
- Spaced repetition scheduling (SM-2-inspired)
- Progress dashboard with Chart.js
- Question library with 4 topic areas
- Responsive Angular Material UI that doesn't look like it's from 2012

---

## Local Development

### Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.x |
| Node.js | 22.x |
| Angular CLI | 18.x (`npm i -g @angular/cli`) |
| EF Core CLI | latest (`dotnet tool install -g dotnet-ef`) |
| PostgreSQL | Neon.tech free tier or local |

### 1 — Clone

```bash
git clone https://github.com/<your-username>/interview-bank.git
cd interview-bank
```

### 2 — API config

Copy the example and fill in your values:

```bash
cp src/InterviewBank.API/appsettings.example.json src/InterviewBank.API/appsettings.json
```

Or use user-secrets (recommended, keeps credentials out of source):

```bash
cd src/InterviewBank.API
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
dotnet user-secrets set "Jwt:Key" "<random-32-char-string>"
dotnet user-secrets set "AllowedOrigin" "http://localhost:4200"
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
```

> The `Anthropic:ApiKey` is optional. Without it the "Check my answer with AI" button will return a 503 and politely refuse to work.  
> Get a key at [console.anthropic.com](https://console.anthropic.com). The AI calls use `claude-haiku-4-5` which is cheap enough that you won't notice it on your bill until you've practiced 10,000 questions (a good problem to have).

### 3 — Apply migrations

```bash
cd src/InterviewBank.API
dotnet ef database update
```

### 4 — Run API

```bash
dotnet run
```

Scalar API docs available at `http://localhost:5000/scalar`.

### 5 — Run Angular

```bash
cd interview-bank-ui
npm install
ng serve
```

App available at `http://localhost:4200`.

---

## Deployment

### Fly.io — API

#### First deploy

```bash
fly auth login
fly launch --name interview-bank-api --region iad --no-deploy
fly secrets set ConnectionStrings__Default="<neon-connection-string>"
fly secrets set Jwt__Key="<random-32-char-string>"
fly secrets set AllowedOrigin="https://interview-bank.vercel.app"
fly secrets set Anthropic__ApiKey="sk-ant-..."
fly deploy
```

#### Subsequent deploys

Handled automatically by GitHub Actions on push to `main`.

### Vercel — Angular SPA

#### First deploy

```bash
cd interview-bank-ui
npm i -g vercel
vercel
```

Follow the prompts. Set `Output Directory` to `dist/interview-bank-ui/browser`.

---

## GitHub Actions Secrets

Add these under **Settings → Secrets and variables → Actions**:

| Secret | How to get it |
|--------|--------------|
| `FLY_API_TOKEN` | `fly tokens create deploy` |
| `VERCEL_TOKEN` | Vercel dashboard → Account → Tokens |
| `VERCEL_ORG_ID` | `vercel env pull` or Vercel dashboard |
| `VERCEL_PROJECT_ID` | `vercel env pull` or Vercel dashboard |

---

## Project Structure

```
interview-bank/
├── .github/workflows/
│   ├── api.yml
│   ├── ui.yml
│   └── pr-check.yml
├── src/
│   └── InterviewBank.API/
│       ├── Controllers/
│       │   ├── AiController.cs        ← essay answer evaluation
│       │   ├── AuthController.cs
│       │   ├── LibraryController.cs
│       │   ├── MockInterviewController.cs
│       │   ├── QuestionsController.cs
│       │   └── TopicsController.cs
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   ├── Migrations/
│       │   └── Seeders/
│       ├── DTOs/
│       ├── Entities/
│       │   ├── Question.cs            ← QuestionType enum (Essay | YesNo)
│       │   └── ...
│       ├── Middleware/
│       └── Services/
├── interview-bank-ui/
│   └── src/app/
│       ├── core/services/
│       │   ├── mock-interview.service.ts  ← AI eval, local sessions
│       │   └── ...
│       ├── features/
│       │   ├── mock-interview/
│       │   │   ├── session-active/    ← Yes/No buttons, AI check panel
│       │   │   ├── session-setup/     ← source selector (bank vs library)
│       │   │   └── session-summary/
│       │   ├── questions/
│       │   │   └── question-form/     ← question type selector
│       │   └── library/
│       └── shared/
├── Dockerfile
├── fly.toml
└── README.md
```

---

## Implementation Phases

| # | Phase | Status |
|---|-------|--------|
| 1 | Project setup & database foundation | ✅ |
| 2 | Authentication — JWT + refresh tokens | ✅ |
| 3 | Questions & topics CRUD | ✅ |
| 4 | Mock interview mode | ✅ |
| 5 | Progress dashboard | ✅ |
| 6 | Deployment, CI/CD | ✅ |
| 7 | Library source in mock interview | ✅ |
| 8 | Yes/No questions + AI essay evaluation | ✅ |

---

## FAQ

**Q: Does the AI actually know if my SQL answer is correct?**  
A: It reads both your answer and the model answer, compares them, and gives you a score and specific feedback. It's not perfect — it's an LLM, not a database — but it's better than asking your dog.

**Q: What happens when the countdown timer hits zero on a Yes/No question?**  
A: It marks it as Missed and shows you the correct answer. The timer has no mercy and neither do interviewers.

**Q: Is my data stored?**  
A: Your questions and session results are saved to your account. Your typed answers are stored per session. The AI evaluation requests are not stored — they go to Anthropic's API and come back as a score. Anthropic's [privacy policy](https://www.anthropic.com/legal/privacy) applies to those calls.

**Q: Can I use this without setting up an Anthropic key?**  
A: Yes. Everything works except the AI accuracy check button. It'll show an error message instead of a score. Not ideal, but the Yes/No instant grading works without AI entirely.

**Q: Why ASP.NET Core and Angular? That's not the most popular stack.**  
A: Exactly.
