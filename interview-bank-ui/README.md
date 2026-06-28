# interview-bank-ui

This folder is the Angular 18 workspace.

## One-time scaffold (run before anything else)

```bash
ng new interview-bank-ui --routing --style=scss --standalone
cd interview-bank-ui
ng add @angular/material
npm install ng2-charts chart.js
```

The CLI generates the base files (`app.component.*`, `app.config.ts`, `app.routes.ts`, etc.).
The subdirectories under `src/app/` — `core/`, `features/`, `shared/` — are pre-created
here so Git tracks them. Populate them starting from **Phase 2**.

## Folder conventions

| Folder | Contents |
|--------|----------|
| `core/services/` | Singleton Angular services (auth, questions, topics, …) |
| `core/interceptors/` | HTTP interceptors (JWT attach) |
| `core/guards/` | Route guards (auth check) |
| `features/` | One subfolder per feature area |
| `shared/components/` | Reusable presentational components |
| `shared/pipes/` | Reusable pipes (e.g. `difficulty-label`) |
