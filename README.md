# Interview Question Bank

Full-stack interview preparation app.  
**Stack:** ASP.NET Core 10 · Angular 18 · PostgreSQL · JWT Auth · $0/month hosting

---

## Quick Start — GitHub Init

```bash
git init
git add .
git commit -m "phase-1: project setup & database foundation"
git branch -M main
git remote add origin https://github.com/<your-username>/interview-bank.git
git push -u origin main
```

---

## Repository Structure

```
interview-bank/
├── src/
│   └── InterviewBank.API/         # ASP.NET Core 10 Web API
│       ├── Entities/              # EF Core entity classes
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   ├── Migrations/        # generated — do not edit by hand
│       │   └── Seeders/
│       └── Program.cs
└── interview-bank-ui/             # Angular 18 SPA — scaffold with ng new
```

---

## Backend Setup

### 1. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://neon.tech) (Neon.tech free tier recommended)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### 2. Connection String (User Secrets — never commit this)

```bash
cd src/InterviewBank.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
```

### 3. Run Migrations

```bash
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
```

The `TopicSeeder` runs automatically on startup and inserts the 13 default topics if they are not already present.

### 4. Run the API

```bash
dotnet run
# Scalar API docs → http://localhost:5000/scalar
```

---

## Frontend Setup

### 1. Scaffold Angular project (run once)

```bash
npm install -g @angular/cli
ng new interview-bank-ui --routing --style=scss --standalone
cd interview-bank-ui
ng add @angular/material
npm install ng2-charts chart.js
```

### 2. Move the generated project into the repo root so it sits alongside `src/`

The `interview-bank-ui/` folder in this repo holds the Angular workspace.  
The pre-created `src/app/` subdirectories (`core/`, `features/`, `shared/`) are placeholders — `ng new` will generate the base files; you then populate the subfolders through phases 2-6.

### 3. Run Angular dev server

```bash
cd interview-bank-ui
ng serve
# → http://localhost:4200
```

---

## Implementation Phases

| # | Phase | Status |
|---|-------|--------|
| 1 | Project Setup & Database Foundation | ✅ this zip |
| 2 | Authentication — Backend & Frontend | ⬜ |
| 3 | Questions & Topics CRUD | ⬜ |
| 4 | Mock Interview Mode | ⬜ |
| 5 | Progress Dashboard | ⬜ |
| 6 | Deployment, CI/CD & README | ⬜ |

---

## Hosting (all free)

| Service | Purpose |
|---------|---------|
| [Neon.tech](https://neon.tech) | PostgreSQL |
| [Fly.io](https://fly.io) | .NET API |
| [Vercel](https://vercel.com) | Angular SPA |
| GitHub Actions | CI/CD |
