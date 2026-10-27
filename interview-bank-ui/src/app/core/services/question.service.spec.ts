import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { QuestionService } from './question.service';
import { environment } from '../../../environments/environment';

describe('QuestionService', () => {
  const api = `${environment.apiUrl}/api/questions`;
  let service: QuestionService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(QuestionService);
    http    = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('serialises every filter into query parameters', () => {
    service.getAll({
      search: 'closure', topicIds: ['t1', 't2'], difficulties: [1, 3], isPracticed: false, page: 2, pageSize: 50
    }).subscribe();

    const params = http.expectOne(r => r.url === api).request.params;
    expect(params.get('search')).toBe('closure');
    expect(params.getAll('topicIds')).toEqual(['t1', 't2']);
    expect(params.getAll('difficulties')).toEqual(['1', '3']);
    expect(params.get('isPracticed')).toBe('false');
    expect(params.get('page')).toBe('2');
    expect(params.get('pageSize')).toBe('50');
  });

  it('omits empty filters', () => {
    service.getAll({ isPracticed: null }).subscribe();

    expect(http.expectOne(api).request.params.keys()).toEqual([]);
  });

  it('uses the right verbs for write operations', () => {
    service.create({ text: 'Q', topicId: 't', difficulty: 1 }).subscribe();
    service.update('q1', { text: 'Q', topicId: 't', difficulty: 2 }).subscribe();
    service.delete('q1').subscribe();
    service.togglePracticed('q1').subscribe();

    expect(http.expectOne({ method: 'POST', url: api })).toBeTruthy();
    expect(http.expectOne({ method: 'PUT', url: `${api}/q1` })).toBeTruthy();
    expect(http.expectOne({ method: 'DELETE', url: `${api}/q1` })).toBeTruthy();
    expect(http.expectOne({ method: 'PATCH', url: `${api}/q1/practiced` })).toBeTruthy();
  });
});
