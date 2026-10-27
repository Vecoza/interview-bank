import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  const api = `${environment.apiUrl}/api/auth`;
  const response = { accessToken: 'token-1', email: 'me@example.com', userId: 'u1' };

  let auth: AuthService;
  let http: HttpTestingController;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: router }
      ]
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('stores the session after login and sends credentials for the refresh cookie', () => {
    auth.login('me@example.com', 'Passw0rd!').subscribe();

    const req = http.expectOne(`${api}/login`);
    expect(req.request.withCredentials).toBeTrue();
    req.flush(response);

    expect(auth.isAuthenticated()).toBeTrue();
    expect(auth.getAccessToken()).toBe('token-1');
    expect(auth.currentEmail()).toBe('me@example.com');
  });

  it('shares a single refresh request between concurrent callers', () => {
    const tokens: string[] = [];
    auth.refresh().subscribe(r => tokens.push(r.accessToken));
    auth.refresh().subscribe(r => tokens.push(r.accessToken));

    http.expectOne(`${api}/refresh`).flush({ ...response, accessToken: 'token-2' });

    expect(tokens).toEqual(['token-2', 'token-2']);
    expect(auth.getAccessToken()).toBe('token-2');
  });

  it('starts a new refresh once the previous one has finished', () => {
    auth.refresh().subscribe();
    http.expectOne(`${api}/refresh`).flush(response);

    let token = '';
    auth.refresh().subscribe(r => (token = r.accessToken));
    http.expectOne(`${api}/refresh`).flush({ ...response, accessToken: 'token-3' });

    expect(token).toBe('token-3');
  });

  it('clears the session and redirects to login when refresh fails', () => {
    auth.login('me@example.com', 'Passw0rd!').subscribe();
    http.expectOne(`${api}/login`).flush(response);

    auth.refresh().subscribe();
    http.expectOne(`${api}/refresh`).flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(auth.isAuthenticated()).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('tryRestore resolves quietly when there is no valid cookie', () => {
    let result: unknown = 'pending';
    auth.tryRestore().subscribe(r => (result = r));

    http.expectOne(`${api}/refresh`).flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(result).toBeNull();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('logout clears the session even if the request fails', () => {
    auth.login('me@example.com', 'Passw0rd!').subscribe();
    http.expectOne(`${api}/login`).flush(response);

    auth.logout();
    http.expectOne(`${api}/logout`).flush({}, { status: 500, statusText: 'Server Error' });

    expect(auth.isAuthenticated()).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
