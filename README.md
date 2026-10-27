# Interview Question Bank

[![API](https://github.com/Vecoza/interview-bank/actions/workflows/api.yml/badge.svg)](https://github.com/Vecoza/interview-bank/actions/workflows/api.yml)
[![UI](https://github.com/Vecoza/interview-bank/actions/workflows/ui.yml/badge.svg)](https://github.com/Vecoza/interview-bank/actions/workflows/ui.yml)

> An interview prep tool built by someone who got tired of losing flashcards and making excuses.
> Now the excuses are automated.

You keep your own bank of interview questions, practise them in timed mock interviews, rate
yourself after every answer, and a spaced-repetition scheduler decides what you should see
again and when. Essay answers can optionally be graded by an LLM against your model answer.

**Stack:** ASP.NET Core (.NET 10) · EF Core + PostgreSQL · Angular 18 + Angular Material · JWT auth · Docker

> The hosted version (Vercel + Fly.io) is currently offline. Everything below runs locally.

---

## How it works

1. **Build a bank.** Add questions with a topic, difficulty (Easy / Medium / Hard) and a model
   answer. Questions are either **Essay** or **Yes / No**. Or skip the typing and import from
   the built-in library (51 questions across JavaScript, C#, SQL and Algorithms).
2. **Run a mock interview.** Pick the number of questions, topics, difficulty, time per
   question (30–180 s) and how questions are picked:
   - *Random*
   - *Least recently practised*
   - *Hardest first*
   - *Due for review* – driven by spaced repetition
3. **Answer against the clock.**
   - *Essay:* type your answer, reveal the model answer, then rate yourself
     **Got it / Partially / Missed it**. Optionally hit **Check my answer** for an AI score
     (0–100) with a sentence or two of feedback.
   - *Yes / No:* two buttons, graded instantly. Run out of time and it counts as missed.
4. **Let the scheduler do its job.** Your rating feeds an SM-2 scheduler: missed questions are
   due again immediately, good answers come back after 1 day, then 6, then at a growing
   interval based on how easy the question has been for you.
5. **Check the dashboard.** Practice coverage per topic and difficulty, a daily streak, your
   weakest topics (by miss rate), recent sessions and how many questions are due.

You can also run a mock interview straight from the library without importing anything. Those
sessions are practice only: they're not saved and don't affect your review schedule.

---

## Running it locally

### Option A: Docker (API + database)

Requires Docker and Node.js 22.

```bash
# optional, enables the "Check my answer" AI grading
export ANTHROPIC_API_KEY=sk-ant-...

docker compose up --build        # API on http://localhost:5050, Postgres on localhost:5433

cd interview-bank-ui
npm install
npm start                        # UI on http://localhost:4200
```

The API applies database migrations and seeds topics and the question library on startup.
API docs (Scalar) are at <http://localhost:5050/scalar>.

### Option B: without Docker

Requires .NET SDK 10, Node.js 22 and a PostgreSQL 15+ database.

```bash
cp src/InterviewBank.API/appsettings.example.json src/InterviewBank.API/appsettings.json
# fill in ConnectionStrings:Default and Jwt:Key (32+ characters)

cd src/InterviewBank.API
dotnet run                       # http://localhost:5050

cd ../../interview-bank-ui
npm install && npm start
```

You can use `dotnet user-secrets` instead of `appsettings.json` if you prefer to keep
credentials out of the project folder.

### Configuration

| Key | Required | Notes |
| --- | --- | --- |
| `ConnectionStrings:Default` | yes | PostgreSQL connection string |
| `Jwt:Key` | yes | HMAC signing key, at least 32 characters |
| `Jwt:Issuer` / `Jwt:Audience` | no | Defaults: `InterviewBank.API` / `InterviewBank.UI` |
| `Jwt:ExpiresInMinutes` | no | Access token lifetime, default 15 |
| `AllowedOrigin` | no | UI origin(s) for CORS, comma separated. Default `http://localhost:4200` |
| `Anthropic:ApiKey` | no | Enables AI grading. Without it the endpoint returns 503 and the rest of the app works normally |
| `Anthropic:Model` | no | Default `claude-haiku-4-5` |
| `Auth:RefreshCookieSameSite` | no | `Strict` in Development, `None` otherwise (UI and API on different domains) |

In Docker / Fly.io use environment variables with `__` as the separator, e.g. `Jwt__Key`.

---

## Tests

```bash
# API: unit + integration tests (needs Docker running, a PostgreSQL container is started automatically)
dotnet test InterviewBank.slnx

# UI: unit tests in headless Chrome
cd interview-bank-ui
npm run test:ci
```

**API (48 tests)** – xUnit. The integration tests boot the real API with
`WebApplicationFactory` against a throwaway PostgreSQL 16 container (Testcontainers), so
migrations, seeders and Postgres-specific queries are exercised for real. Covered:

- auth: register / login, refresh-token rotation and replay protection, logout revocation, tampered tokens
- questions: CRUD, filters and paging, per-user isolation, Yes/No validation
- topics: seeded system topics, private custom topics, delete protection
- library: seeding, filtering, import and de-duplication
- mock interviews: question selection, model answers, completing a session, spaced-repetition
  updates, "due for review" sessions, ownership and double-submit checks
- dashboard statistics, AI grading endpoint (with a fake evaluator), SM-2 maths, AI response parsing

**UI (38 tests)** – Jasmine + Karma: auth service (incl. shared token refresh), HTTP
interceptor, route guards, question and mock-interview services, the countdown timer and the
active-session screen (Yes/No grading, timeouts, AI check, submitting and retrying).

Both suites run in GitHub Actions on every push and pull request.

---

## Project structure

```
interview-bank/
├── src/InterviewBank.API/        ASP.NET Core Web API
│   ├── Controllers/              Auth, Questions, Topics, Library, MockInterview, Dashboard, Ai
│   ├── Services/                 TokenService, MockInterviewService, SpacedRepetitionService, AnswerEvaluator
│   ├── Entities/ DTOs/           domain model and request/response contracts
│   ├── Data/                     DbContext, migrations, seeders (topics + question library)
│   └── Middleware/               global exception handler
├── tests/InterviewBank.API.Tests/
│   ├── Unit/                     SM-2 scheduler, AI response parsing
│   └── Integration/              HTTP-level tests against PostgreSQL
├── interview-bank-ui/            Angular 18 SPA (standalone components, signals)
│   └── src/app/
│       ├── core/                 auth service, interceptor, guards, API services
│       ├── features/             auth, questions, library, mock-interview, dashboard
│       └── shared/               timer, badges, dialogs, pipes
├── Dockerfile                    API image
├── docker-compose.yml            local API + PostgreSQL
├── fly.toml                      Fly.io config
└── .github/workflows/            api.yml, ui.yml
```

### A few design notes

- **Auth:** short-lived JWT access token kept in memory only, plus an HttpOnly refresh-token
  cookie that rotates on every use. The UI restores the session on page load and shares one
  refresh call between parallel requests, since a refresh token can only be used once.
- **Mock interview sessions** are stored in the database as soon as they start, so finishing
  one doesn't depend on server memory and survives restarts.
- **Data isolation:** every query is scoped to the signed-in user; custom topics are private.

---

## Deployment

CI builds and tests both apps on every push. Deploys run on pushes to `main` and are skipped
(not failed) when the secrets below are missing.

| Secret | Used for |
| --- | --- |
| `FLY_API_TOKEN` | API → Fly.io (`fly tokens create deploy`) |
| `VERCEL_TOKEN`, `VERCEL_ORG_ID`, `VERCEL_PROJECT_ID` | UI → Vercel |

First API deploy:

```bash
fly launch --name interview-bank-api --region iad --no-deploy
fly secrets set ConnectionStrings__Default="<postgres-connection-string>" \
                Jwt__Key="<random-32+-char-string>" \
                AllowedOrigin="https://<your-ui-domain>" \
                Anthropic__ApiKey="<optional>"
fly deploy
```

For the UI, point `src/environments/environment.prod.ts` at your API URL and set the Vercel
output directory to `dist/interview-bank-ui/browser`.

Because the UI and API live on different domains, the refresh cookie is sent as
`SameSite=None; Secure`. Browsers that block third-party cookies (Safari by default) won't keep
you signed in across page reloads; serving the API from a subdomain of the UI's domain avoids that.

---

## FAQ

**Does the AI actually know if my SQL answer is correct?**
It compares your answer with the model answer and returns a score and specific feedback. It's
not perfect, it's a language model rather than a database, but it's better than asking your dog.

**What happens when the timer hits zero on a Yes/No question?**
It's marked as missed and the correct answer is shown. The timer has no mercy, and neither do interviewers.

**Is my data stored?**
Questions, sessions and your typed answers are saved to your account. Text sent for AI grading
goes to the Anthropic API and isn't stored by this app.

**Can I use it without an AI key?**
Yes. Everything except the "Check my answer" button works without one.

**Why ASP.NET Core and Angular? That's not the most popular stack.**
Exactly.
