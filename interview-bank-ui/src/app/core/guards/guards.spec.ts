import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { signal } from '@angular/core';
import { authGuard } from './auth.guard';
import { guestGuard } from './guest.guard';
import { AuthService } from '../services/auth.service';

describe('route guards', () => {
  const loggedIn = signal(false);

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { isAuthenticated: loggedIn } }
      ]
    });
  });

  const run = (guard: typeof authGuard) =>
    TestBed.runInInjectionContext(() => guard({} as any, {} as any));

  const url = (result: unknown) => TestBed.inject(Router).serializeUrl(result as UrlTree);

  it('authGuard lets signed-in users through', () => {
    loggedIn.set(true);
    expect(run(authGuard)).toBeTrue();
  });

  it('authGuard sends anonymous users to /login', () => {
    loggedIn.set(false);
    expect(url(run(authGuard))).toBe('/login');
  });

  it('guestGuard keeps signed-in users away from login pages', () => {
    loggedIn.set(true);
    expect(url(run(guestGuard))).toBe('/questions');
  });

  it('guestGuard allows anonymous users', () => {
    loggedIn.set(false);
    expect(run(guestGuard)).toBeTrue();
  });
});
