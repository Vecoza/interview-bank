import { ComponentFixture, TestBed, fakeAsync, tick, discardPeriodicTasks } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { SessionActiveComponent } from './session-active.component';
import { ActiveSession, MockInterviewService } from '../../../core/services/mock-interview.service';
import { environment } from '../../../../environments/environment';

describe('SessionActiveComponent', () => {
  const api = `${environment.apiUrl}/api`;

  const session: ActiveSession = {
    sessionId: 'session-1',
    timePerQuestion: 30,
    questions: [
      { id: 'q1', text: 'Is NaN equal to NaN?', topicName: 'JavaScript', difficulty: 1, questionOrder: 1,
        questionType: 1, correctAnswer: 'No' },
      { id: 'q2', text: 'What is a closure?', topicName: 'JavaScript', difficulty: 2, questionOrder: 2,
        questionType: 0, expectedAnswer: 'A function bundled with its lexical scope.' }
    ]
  };

  let fixture: ComponentFixture<SessionActiveComponent>;
  let component: SessionActiveComponent;
  let service: MockInterviewService;
  let http: HttpTestingController;
  let router: jasmine.SpyObj<Router>;

  function create(active: ActiveSession | null = session) {
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    TestBed.configureTestingModule({
      imports: [SessionActiveComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        { provide: Router, useValue: router }
      ]
    });
    service = TestBed.inject(MockInterviewService);
    http    = TestBed.inject(HttpTestingController);
    if (active) service.setActiveSession(active);

    fixture   = TestBed.createComponent(SessionActiveComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  const text = () => (fixture.nativeElement as HTMLElement).textContent ?? '';

  afterEach(() => {
    component?.ngOnDestroy();
    http?.verify();
  });

  it('redirects to setup when there is no active session', () => {
    create(null);
    expect(router.navigate).toHaveBeenCalledWith(['/mock-interview']);
  });

  it('grades a correct yes/no answer instantly', () => {
    create();
    component.answerYesNo('No');

    expect(component.yesNoCorrect()).toBeTrue();
    expect(component.selfAssessment()).toBe(1);
    expect(component.isRevealed()).toBeTrue();
  });

  it('grades a wrong yes/no answer as missed', () => {
    create();
    component.answerYesNo('Yes');

    expect(component.yesNoCorrect()).toBeFalse();
    expect(component.selfAssessment()).toBe(3);
  });

  it('marks an unanswered yes/no question as missed when time runs out', fakeAsync(() => {
    create();

    tick(30_000);

    expect(component.isRevealed()).toBeTrue();
    expect(component.selfAssessment()).toBe(3);
    discardPeriodicTasks();
  }));

  it('shows the model answer after revealing an essay question', () => {
    create();
    component.answerYesNo('No');
    component.next();

    component.revealAnswer();
    fixture.detectChanges();

    expect(text()).toContain('A function bundled with its lexical scope.');
  });

  it('sends the model answer to AI grading and shows the score', () => {
    create();
    component.answerYesNo('No');
    component.next();
    component.userAnswer.set('A function that remembers variables.');
    component.revealAnswer();

    component.checkWithAi();
    const req = http.expectOne(`${api}/ai/evaluate-answer`);
    expect(req.request.body).toEqual({
      questionText:   'What is a closure?',
      expectedAnswer: 'A function bundled with its lexical scope.',
      userAnswer:     'A function that remembers variables.'
    });
    req.flush({ score: 75, feedback: 'Mention lexical scope.' });
    fixture.detectChanges();

    expect(component.aiResult()?.score).toBe(75);
    expect(text()).toContain('Mention lexical scope.');
  });

  it('shows the server error when AI grading is unavailable', () => {
    create();
    component.answerYesNo('No');
    component.next();
    component.userAnswer.set('Something');
    component.revealAnswer();

    component.checkWithAi();
    http.expectOne(`${api}/ai/evaluate-answer`)
      .flush({ error: 'AI evaluation is not configured on the server.' }, { status: 503, statusText: 'Unavailable' });

    expect(component.aiError()).toBe('AI evaluation is not configured on the server.');
  });

  it('submits every answer once when the session finishes', () => {
    create();
    component.answerYesNo('No');
    component.next();
    component.revealAnswer();
    component.rate(2);
    component.next();

    const req = http.expectOne(`${api}/mock-interview/complete`);
    expect(req.request.body.sessionId).toBe('session-1');
    expect(req.request.body.answers.map((a: any) => [a.questionId, a.selfAssessment])).toEqual([['q1', 1], ['q2', 2]]);
    req.flush({ sessionId: 'session-1', results: [] });

    expect(router.navigate).toHaveBeenCalledWith(['/mock-interview/summary']);
  });

  it('can retry a failed save without duplicating answers', () => {
    create();
    component.answerYesNo('No');
    component.next();
    component.revealAnswer();
    component.rate(1);

    component.next();
    http.expectOne(`${api}/mock-interview/complete`)
      .flush({ error: 'Session not found or already completed.' }, { status: 400, statusText: 'Bad Request' });
    expect(component.submitError()).toBe('Session not found or already completed.');

    component.next();
    const retry = http.expectOne(`${api}/mock-interview/complete`);
    expect(retry.request.body.answers.length).toBe(2);
    retry.flush({});
  });

  it('completes library sessions locally without calling the API', () => {
    create(null);
    service.startLocalSession([
      { id: 'l1', topicName: 'SQL', difficulty: 1, text: 'What is a JOIN?', expectedAnswer: 'Combines rows.', alreadyImported: false }
    ], 60);
    component.ngOnInit();

    component.revealAnswer();
    component.rate(1);
    component.next();

    expect(service.sessionSummary()?.gotItCount).toBe(1);
    expect(router.navigate).toHaveBeenCalledWith(['/mock-interview/summary']);
  });
});
