# Interview Question Bank

A full-stack interview preparation app built as a portfolio project.

**Stack:** ASP.NET Core 10 · Angular 18 · PostgreSQL (Neon.tech) · JWT Auth · Chart.js  
**Hosting:** Fly.io (API) · Vercel (UI) · GitHub Actions (CI/CD) — all free tier

---

## Live

| Service | URL |
|---------|-----|
| Angular SPA | `https://interview-bank.vercel.app` |
| .NET API | `https://interview-bank-api.fly.dev` |
| API docs (Scalar) | `https://interview-bank-api.fly.dev/scalar` |

---

## Features

- JWT authentication with HttpOnly refresh-token rotation
- Full question CRUD — filter by topic, difficulty, practiced state with URL-synced query params
- Mock Interview mode — countdown timer, self-assessment, three question-selection strategies
- Progress Dashboard — Chart.js difficulty and topic charts, streak, weakest topics
- Responsive Angular Material UI

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

### 2 — API secrets (never committed)

```bash
cd src/InterviewBank.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
dotnet user-secrets set "JwtSecret" "<random-32-char-string>"
dotnet user-secrets set "AllowedOrigin" "http://localhost:4200"
```

### 3 — Apply migrations

```bash
dotnet ef database update
```

### 4 — Run API

```bash
dotnet run
```

Scalar docs available at `http://localhost:5000/scalar`.

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
fly secrets set JwtSecret="<random-32-char-string>"
fly secrets set AllowedOrigin="https://interview-bank.vercel.app"
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

#### Environment variable

Set `apiUrl` via `environment.prod.ts` (already points to `interview-bank-api.fly.dev`).  
Rebuild and redeploy after any change.

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

## CI/CD

| Workflow | Trigger | Jobs |
|----------|---------|------|
| `api.yml` | Push to `main` (src changes) | Build → Deploy to Fly.io |
| `ui.yml` | Push to `main` (ui changes) | Build → Deploy to Vercel |
| `pr-check.yml` | Any PR to `main` | Build check for API and UI |

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
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   ├── Migrations/
│       │   └── Seeders/
│       ├── DTOs/
│       ├── Entities/
│       ├── Middleware/
│       └── Services/
├── interview-bank-ui/
│   └── src/app/
│       ├── core/
│       │   ├── guards/
│       │   ├── interceptors/
│       │   └── services/
│       ├── features/
│       │   ├── auth/
│       │   ├── dashboard/
│       │   ├── mock-interview/
│       │   └── questions/
│       └── shared/
│           ├── components/
│           └── pipes/
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
| 6 | Deployment, CI/CD & README | ✅ |
