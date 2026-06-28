import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface LibraryQuestion {
  id:              string;
  topicName:       string;
  difficulty:      number;
  text:            string;
  expectedAnswer?: string;
}

@Injectable({ providedIn: 'root' })
export class LibraryService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/library`;

  getAll(topicName?: string, difficulty?: number) {
    let params = new HttpParams();
    if (topicName)  params = params.set('topicName', topicName);
    if (difficulty) params = params.set('difficulty', difficulty);
    return this.http.get<LibraryQuestion[]>(this.base, { params });
  }

  import(ids: string[]) {
    return this.http.post<{ imported: number }>(`${this.base}/import`, { ids });
  }
}
