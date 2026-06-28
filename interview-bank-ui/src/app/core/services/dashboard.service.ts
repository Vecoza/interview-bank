import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DifficultyBreakdown {
  total: number;
  practiced: number;
}

export interface TopicBreakdown {
  topicName: string;
  total: number;
  practiced: number;
}

export interface WeakTopic {
  topicName: string;
  missedCount: number;
  totalAnswered: number;
  missedRatio: number;
}

export interface RecentSession {
  sessionId: string;
  completedAt: string;
  totalQuestions: number;
  gotItCount: number;
  partialCount: number;
  missedCount: number;
}

export interface DashboardStats {
  totalQuestions: number;
  practicedCount: number;
  byDifficulty: Record<string, DifficultyBreakdown>;
  byTopic: TopicBreakdown[];
  recentSessions: RecentSession[];
  weakestTopics: WeakTopic[];
  practiceStreak:    number;
  dueForReviewCount: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly api = `${environment.apiUrl}/api/dashboard`;

  readonly stats   = signal<DashboardStats | null>(null);
  readonly loading = signal(false);

  constructor(private http: HttpClient) {}

  load() {
    this.loading.set(true);
    return this.http.get<DashboardStats>(`${this.api}/stats`).pipe(
      tap(data => {
        this.stats.set(data);
        this.loading.set(false);
      })
    );
  }
}
