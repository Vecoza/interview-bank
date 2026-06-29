import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface Question {
  id: string;
  userId: string;
  topicId: string;
  topicName: string;
  text: string;
  difficulty: number;
  difficultyLabel: string;
  questionType: number;
  expectedAnswer: string | null;
  personalNotes: string | null;
  source: string | null;
  isPracticed: boolean;
  practiceCount: number;
  lastPracticedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface QuestionFilter {
  search?: string;
  topicIds?: string[];
  difficulties?: number[];
  isPracticed?: boolean | null;
  page?: number;
  pageSize?: number;
}

export interface PaginatedQuestions {
  total: number;
  page: number;
  pageSize: number;
  questions: Question[];
}

export interface CreateQuestion {
  text: string;
  topicId: string;
  difficulty: number;
  questionType?: number;
  expectedAnswer?: string;
  personalNotes?: string;
  source?: string;
}

@Injectable({ providedIn: 'root' })
export class QuestionService {
  private readonly api = `${environment.apiUrl}/api/questions`;

  constructor(private http: HttpClient) {}

  getAll(filter: QuestionFilter = {}) {
    let params = new HttpParams();

    if (filter.search)        params = params.set('search', filter.search);
    if (filter.isPracticed != null)
                              params = params.set('isPracticed', String(filter.isPracticed));
    if (filter.page)          params = params.set('page', String(filter.page));
    if (filter.pageSize)      params = params.set('pageSize', String(filter.pageSize));

    filter.topicIds?.forEach(id  => { params = params.append('topicIds', id); });
    filter.difficulties?.forEach(d => { params = params.append('difficulties', String(d)); });

    return this.http.get<PaginatedQuestions>(this.api, { params });
  }

  getById(id: string) {
    return this.http.get<Question>(`${this.api}/${id}`);
  }

  create(dto: CreateQuestion) {
    return this.http.post<Question>(this.api, dto);
  }

  update(id: string, dto: CreateQuestion) {
    return this.http.put<Question>(`${this.api}/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete(`${this.api}/${id}`);
  }

  togglePracticed(id: string) {
    return this.http.patch<Question>(`${this.api}/${id}/practiced`, {});
  }
}
