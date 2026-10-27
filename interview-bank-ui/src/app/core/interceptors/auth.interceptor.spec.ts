import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { of, EMPTY } from 'rxjs';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let auth: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['getAccessToken', 'refresh']);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth }
      ]
    });
    http    = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('attaches the bearer token when signed in', () => {
    auth.getAccessToken.and.returnValue('abc');
    http.get('/api/questions').subscribe();

    expect(backend.expectOne('/api/questions').request.headers.get('Authorization')).toBe('Bearer abc');
  });

  it('sends no header when signed out', () => {
    auth.getAccessToken.and.returnValue(null);
    http.get('/api/library').subscribe();

    expect(backend.expectOne('/api/library').request.headers.has('Authorization')).toBeFalse();
  });

  it('refreshes once on 401 and retries with the new token', () => {
    auth.getAccessToken.and.returnValue('expired');
    auth.refresh.and.returnValue(of({ accessToken: 'fresh', email: 'a@b.c', userId: 'u' }));

    let body: unknown;
    http.get('/api/questions').subscribe(r => (body = r));

    backend.expectOne('/api/questions').flush({}, { status: 401, statusText: 'Unauthorized' });
    const retry = backend.expectOne('/api/questions');
    expect(retry.request.headers.get('Authorization')).toBe('Bearer fresh');
    retry.flush({ ok: true });

    expect(body).toEqual({ ok: true });
    expect(auth.refresh).toHaveBeenCalledTimes(1);
  });

  it('completes without retrying when the refresh fails', () => {
    auth.getAccessToken.and.returnValue('expired');
    auth.refresh.and.returnValue(EMPTY);

    let completed = false;
    http.get('/api/questions').subscribe({ complete: () => (completed = true) });
    backend.expectOne('/api/questions').flush({}, { status: 401, statusText: 'Unauthorized' });

    backend.expectNone('/api/questions');
    expect(completed).toBeTrue();
  });

  it('does not try to refresh failed auth calls', () => {
    auth.getAccessToken.and.returnValue(null);
    let status = 0;
    http.post('/api/auth/login', {}).subscribe({ error: e => (status = e.status) });

    backend.expectOne('/api/auth/login').flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(status).toBe(401);
    expect(auth.refresh).not.toHaveBeenCalled();
  });

  it('passes other errors straight through', () => {
    auth.getAccessToken.and.returnValue('abc');
    let status = 0;
    http.get('/api/questions').subscribe({ error: e => (status = e.status) });

    backend.expectOne('/api/questions').flush({}, { status: 500, statusText: 'Server Error' });

    expect(status).toBe(500);
    expect(auth.refresh).not.toHaveBeenCalled();
  });
});
