import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Insight {
  insightId: number;
  title: string;
  summary: string;
  topicName: string;
  urgencyLevel: string;
  feedbackCount: number;
}

@Injectable({ providedIn: 'root' })
export class InsightService {
  private apiUrl = 'http://localhost:5157/api/insights';

  constructor(private http: HttpClient) {}

  // Gets all AI-generated insights — used in insights page
  getAll(): Observable<Insight[]> {
    return this.http.get<Insight[]>(this.apiUrl);
  }

  // Triggers AI to generate insights from analyzed feedbacks
  generate(): Observable<any> {
    return this.http.post(`${this.apiUrl}/generate`, {});
  }
}