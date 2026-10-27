import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MockInterviewService, SubmitAnswer } from './mock-interview.service';
import { LibraryQuestion } from './library.service';
import { environment } from '../../../environments/environment';

describe('MockInterviewService', () => {
  let service: MockInterviewService;
  let http: HttpTestingController;

  const library: LibraryQuestion[] = [
    { id: 'l1', topicName: 'SQL', difficulty: 1, text: 'What is a JOIN?', expectedAnswer: 'Combines rows.', alreadyImported: false },
    { id: 'l2', topicName: 'C#',  difficulty: 3, text: 'What is a Span?', expectedAnswer: 'A memory view.', alreadyImported: false }
  ];

  const answer = (questionId: string, selfAssessment: number, timeTakenSeconds = 30): SubmitAnswer =>
    ({ sessionId: 's', questionId, selfAssessment, timeTakenSeconds });

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(MockInterviewService);
    http    = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('builds a local session from library questions, keeping model answers', () => {
    service.startLocalSession(library, 90);

    const session = service.activeSession()!;
    expect(service.isLocalSession()).toBeTrue();
    expect(session.timePerQuestion).toBe(90);
    expect(session.questions.map(q => q.questionOrder)).toEqual([1, 2]);
    expect(session.questions[1].expectedAnswer).toBe('A memory view.');
  });

  it('summarises a local session without calling the API', () => {
    service.startLocalSession(library, 60);

    const summary = service.completeLocal([answer('l1', 1, 20), answer('l2', 3, 41)]);

    expect(summary.totalQuestions).toBe(2);
    expect([summary.gotItCount, summary.partialCount, summary.missedCount]).toEqual([1, 0, 1]);
    expect(summary.averageTimeTaken).toBe(31);
    expect(summary.results[1].expectedAnswer).toBe('A memory view.');
  });

  it('switching to a server session resets the local state', () => {
    service.startLocalSession(library, 60);

    service.setActiveSession({ sessionId: 'server', timePerQuestion: 60, questions: [] });

    expect(service.isLocalSession()).toBeFalse();
  });

  it('posts start and complete requests to the API', () => {
    service.start({ questionCount: 5, strategy: 3, timePerQuestion: 60 }).subscribe();
    service.complete({ sessionId: 's', answers: [answer('q1', 2)] }).subscribe();

    const start = http.expectOne(`${environment.apiUrl}/api/mock-interview/start`);
    expect(start.request.body.strategy).toBe(3);
    const complete = http.expectOne(`${environment.apiUrl}/api/mock-interview/complete`);
    expect(complete.request.body.answers.length).toBe(1);
  });
});
