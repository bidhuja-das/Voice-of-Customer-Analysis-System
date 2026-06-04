import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SentimentSummary {
  positive: number;
  negative: number;
  neutral: number;
  total: number;
  positivePercent: number;
  negativePercent: number;
  neutralPercent: number;
}

export interface TopicData {
  topicName: string;
  count: number;
  positiveCount: number;
  negativeCount: number;
  neutralCount: number;
}

export interface ComparisonData {
  period: string;
  positive: number;
  negative: number;
  neutral: number;
  total: number;
}

export interface DailyReport {
  reportId: number;
  reportDate: string;
  totalFeedback: number;
  positiveCount: number;
  negativeCount: number;
  neutralCount: number;
  topIssue: string;
  summary: string;
}

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private apiUrl = 'http://localhost:5157/api/analytics';
  private dashboardUrl = 'http://localhost:5157/api/dashboard';

  constructor(private http: HttpClient) {}

  getSentimentSummary(from?: string, to?: string): Observable<SentimentSummary> {
    let params = '';
    if (from && to) params = `?from=${from}&to=${to}`;
    return this.http.get<SentimentSummary>(`${this.apiUrl}/sentiment-summary${params}`);
  }

  getTopics(from?: string, to?: string): Observable<TopicData[]> {
    let params = '';
    if (from && to) params = `?from=${from}&to=${to}`;
    return this.http.get<TopicData[]>(`${this.apiUrl}/topics${params}`);
  }

  getComparison(from?: string, to?: string): Observable<ComparisonData[]> {
  let params = '';
  if (from && to) params = `?from=${from}&to=${to}`;
  return this.http.get<ComparisonData[]>(`${this.apiUrl}/comparison${params}`);
}

  getDailyReports(): Observable<DailyReport[]> {
    return this.http.get<DailyReport[]>(`${this.apiUrl}/daily-reports`);
  }

  getDashboardStats(from?: string, to?: string): Observable<any> {
    let params = '';
    if (from && to) params = `?from=${from}&to=${to}`;
    return this.http.get<any>(`${this.dashboardUrl}/stats${params}`);
  }
}