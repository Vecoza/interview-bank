import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'questions', pathMatch: 'full' },

  // ── Public routes ───────────────────────────────────────────────────────────
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },

  // ── Protected routes — populated in Phase 3-5 ───────────────────────────────
  {
    path: 'questions',
    canActivate: [authGuard],
    // loadChildren added in Phase 3
    loadComponent: () =>
      import('./features/questions/question-list/question-list.component')
        .then(m => m.QuestionListComponent)
  },
  {
    path: 'mock-interview',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/mock-interview/session-setup/session-setup.component')
        .then(m => m.SessionSetupComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard.component')
        .then(m => m.DashboardComponent)
  },

  { path: '**', redirectTo: 'questions' }
];
