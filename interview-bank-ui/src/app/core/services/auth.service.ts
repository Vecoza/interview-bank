import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap, catchError, EMPTY, of, Observable, finalize, shareReplay } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AuthResponse {
  accessToken: string;
  email: string;
  userId: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = `${environment.apiUrl}/api/auth`;

  private _accessToken = signal<string | null>(null);
  private _email       = signal<string | null>(null);
  private _userId      = signal<string | null>(null);

  readonly isAuthenticated = computed(() => this._accessToken() !== null);
  readonly currentEmail    = this._email.asReadonly();
  readonly currentUserId   = this._userId.asReadonly();

  getAccessToken(): string | null {
    return this._accessToken();
  }

  constructor(private http: HttpClient, private router: Router) {}

  register(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${this.api}/register`, { email, password }, { withCredentials: true })
      .pipe(tap(res => this.setSession(res)));
  }

  login(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${this.api}/login`, { email, password }, { withCredentials: true })
      .pipe(tap(res => this.setSession(res)));
  }

  logout() {
    this.http
      .post(`${this.api}/logout`, {}, { withCredentials: true })
      .pipe(
        catchError(() => EMPTY),
        finalize(() => {
          this.clearSession();
          this.router.navigate(['/login']);
        })
      )
      .subscribe();
  }

  private refreshInFlight: Observable<AuthResponse> | null = null;

  /**
   * Refresh tokens are single-use, so concurrent 401s must share one refresh call;
   * otherwise the second call presents an already-rotated token and logs the user out.
   */
  refresh(): Observable<AuthResponse> {
    if (!this.refreshInFlight) {
      this.refreshInFlight = this.http
        .post<AuthResponse>(`${this.api}/refresh`, {}, { withCredentials: true })
        .pipe(
          tap(res => this.setSession(res)),
          catchError(() => {
            this.clearSession();
            this.router.navigate(['/login']);
            return EMPTY;
          }),
          finalize(() => (this.refreshInFlight = null)),
          shareReplay(1)
        );
    }
    return this.refreshInFlight;
  }

  tryRestore() {
    return this.http
      .post<AuthResponse>(`${this.api}/refresh`, {}, { withCredentials: true })
      .pipe(
        tap(res => this.setSession(res)),
        catchError(() => of(null))
      );
  }

  private setSession(res: AuthResponse): void {
    this._accessToken.set(res.accessToken);
    this._email.set(res.email);
    this._userId.set(res.userId);
  }

  private clearSession(): void {
    this._accessToken.set(null);
    this._email.set(null);
    this._userId.set(null);
  }
}
