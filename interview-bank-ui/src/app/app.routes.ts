import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'questions', pathMatch: 'full' },

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

  {
    path: 'questions',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/questions/question-list/question-list.component')
        .then(m => m.QuestionListComponent)
  },
  {
    path: 'questions/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/questions/question-form/question-form.component')
        .then(m => m.QuestionFormComponent)
  },
  {
    path: 'questions/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/questions/question-detail/question-detail.component')
        .then(m => m.QuestionDetailComponent)
  },
  {
    path: 'questions/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/questions/question-form/question-form.component')
        .then(m => m.QuestionFormComponent)
  },

  {
    path: 'mock-interview',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/mock-interview/session-setup/session-setup.component')
        .then(m => m.SessionSetupComponent)
  },
  {
    path: 'mock-interview/active',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/mock-interview/session-active/session-active.component')
        .then(m => m.SessionActiveComponent)
  },
  {
    path: 'mock-interview/summary',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/mock-interview/session-summary/session-summary.component')
        .then(m => m.SessionSummaryComponent)
  },

  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard.component')
        .then(m => m.DashboardComponent)
  },

  {
    path: 'library',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/library/library.component')
        .then(m => m.LibraryComponent)
  },

  { path: '**', redirectTo: 'questions' }
];
