import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LibraryQuestion } from './library.service';

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
  questionType: number;
  correctAnswer?: string;
}

export interface AiEvaluation {
  score: number;
  feedback: string;
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
  readonly isLocalSession = signal(false);

  private localLibraryQuestions: LibraryQuestion[] = [];

  constructor(private http: HttpClient) {}

  getDueCount() {
    return this.http.get<{ count: number }>(`${this.api}/due-count`);
  }

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

  evaluateAnswer(questionText: string, expectedAnswer: string, userAnswer: string) {
    return this.http.post<AiEvaluation>(`${environment.apiUrl}/api/ai/evaluate-answer`, {
      questionText, expectedAnswer, userAnswer
    }, { withCredentials: true });
  }

  getLocalExpectedAnswer(questionId: string): string | null {
    return this.localLibraryQuestions.find(q => q.id === questionId)?.expectedAnswer ?? null;
  }

  startLocalSession(questions: LibraryQuestion[], timePerQuestion: number) {
    this.localLibraryQuestions = questions;
    const sessionQuestions: SessionQuestion[] = questions.map((q, i) => ({
      id: q.id,
      text: q.text,
      topicName: q.topicName,
      difficulty: q.difficulty,
      questionOrder: i + 1,
      questionType: 0,
      correctAnswer: undefined
    }));
    this.isLocalSession.set(true);
    this.activeSession.set({
      sessionId: `local-${Date.now()}`,
      timePerQuestion,
      questions: sessionQuestions
    });
  }

  completeLocal(answers: SubmitAnswer[]): SessionSummary {
    const session = this.activeSession()!;
    const gotItCount  = answers.filter(a => a.selfAssessment === 1).length;
    const partialCount = answers.filter(a => a.selfAssessment === 2).length;
    const missedCount  = answers.filter(a => a.selfAssessment === 3).length;
    const avgTime = answers.length
      ? answers.reduce((s, a) => s + a.timeTakenSeconds, 0) / answers.length
      : 0;

    const results: SessionResultItem[] = answers.map((a, i) => {
      const libQ = this.localLibraryQuestions.find(q => q.id === a.questionId);
      return {
        questionText:     libQ?.text ?? '',
        topicName:        libQ?.topicName ?? '',
        difficulty:       libQ?.difficulty ?? 1,
        userAnswer:       a.userAnswer ?? null,
        expectedAnswer:   libQ?.expectedAnswer ?? null,
        timeTakenSeconds: a.timeTakenSeconds,
        selfAssessment:   a.selfAssessment,
        questionOrder:    i + 1
      };
    });

    return {
      sessionId:         session.sessionId,
      startedAt:         new Date().toISOString(),
      completedAt:       new Date().toISOString(),
      totalQuestions:    answers.length,
      timePerQuestion:   session.timePerQuestion,
      gotItCount,
      partialCount,
      missedCount,
      averageTimeTaken:  Math.round(avgTime),
      results
    };
  }

  clearSession() {
    this.activeSession.set(null);
    this.sessionSummary.set(null);
    this.isLocalSession.set(false);
    this.localLibraryQuestions = [];
  }
}
