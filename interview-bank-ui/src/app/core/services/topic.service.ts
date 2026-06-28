import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Topic {
  id: string;
  name: string;
  isSystem: boolean;
  questionCount: number;
}

@Injectable({ providedIn: 'root' })
export class TopicService {
  private readonly api = `${environment.apiUrl}/api/topics`;

  // Cached topics — shared across components to avoid redundant API calls
  readonly topics = signal<Topic[]>([]);

  constructor(private http: HttpClient) {}

  load() {
    return this.http.get<Topic[]>(this.api).pipe(
      tap(topics => this.topics.set(topics))
    );
  }

  create(name: string) {
    return this.http.post<Topic>(this.api, { name }).pipe(
      tap(topic => this.topics.update(list => [...list, topic]))
    );
  }

  delete(id: string) {
    return this.http.delete(`${this.api}/${id}`).pipe(
      tap(() => this.topics.update(list => list.filter(t => t.id !== id)))
    );
  }
}
