import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface StartSessionRequest {
  questionCount: number;
  topicIds?: string[];
  difficulties?: number[];
  strategy: number;
  timePerQuestion: number;
}

export interface SessionQuestion {
  id: string;
  text: string;
  topicName: string;
  difficulty: number;
  questionOrder: number;
}

export interface ActiveSession {
  sessionId: string;
  timePerQuestion: number;
  questions: SessionQuestion[];
}

export interface SubmitAnswer {
  sessionId: string;
  questionId: string;
  userAnswer?: string;
  timeTakenSeconds: number;
  selfAssessment: number;
}

export interface CompleteSessionRequest {
  sessionId: string;
  answers: SubmitAnswer[];
}

export interface SessionResultItem {
  questionText: string;
  topicName: string;
  difficulty: number;
  userAnswer: string | null;
  expectedAnswer: string | null;
  timeTakenSeconds: number;
  selfAssessment: number;
  questionOrder: number;
}

export interface SessionSummary {
  sessionId: string;
  startedAt: string;
  completedAt: string;
  totalQuestions: number;
  timePerQuestion: number;
  gotItCount: number;
  partialCount: number;
  missedCount: number;
  averageTimeTaken: number;
  results: SessionResultItem[];
}

export interface PastSession {
  sessionId: string;
  completedAt: string;
  totalQuestions: number;
  gotItCount: number;
  partialCount: number;
  missedCount: number;
}

@Injectable({ providedIn: 'root' })
export class MockInterviewService {
  private readonly api = `${environment.apiUrl}/api/mock-interview`;

  readonly activeSession  = signal<ActiveSession | null>(null);
  readonly sessionSummary = signal<SessionSummary | null>(null);

  constructor(private http: HttpClient) {}

  start(request: StartSessionRequest) {
    return this.http.post<ActiveSession>(`${this.api}/start`, request, { withCredentials: true });
  }

  complete(request: CompleteSessionRequest) {
    return this.http.post<SessionSummary>(`${this.api}/complete`, request, { withCredentials: true });
  }

  getSessions() {
    return this.http.get<PastSession[]>(`${this.api}/sessions`);
  }

  setActiveSession(session: ActiveSession) {
    this.activeSession.set(session);
  }

  setSummary(summary: SessionSummary) {
    this.sessionSummary.set(summary);
  }

  clearSession() {
    this.activeSession.set(null);
    this.sessionSummary.set(null);
  }
}
