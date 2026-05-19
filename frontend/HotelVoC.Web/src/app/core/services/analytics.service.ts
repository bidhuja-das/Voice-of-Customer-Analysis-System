import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SentimentSummary {
  positive: number;
  negative: number;
  neutral: number;
  total: number;
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

  // Gets % positive/negative/neutral — used in pie chart
  getSentimentSummary(): Observable<SentimentSummary> {
    return this.http.get<SentimentSummary>(`${this.apiUrl}/sentiment-summary`);
  }

  // Gets topic list with counts — used in bar chart
  getTopics(): Observable<TopicData[]> {
    return this.http.get<TopicData[]>(`${this.apiUrl}/topics`);
  }

  // Gets day/week/month comparison — used in comparison chart
  getComparison(): Observable<ComparisonData[]> {
    return this.http.get<ComparisonData[]>(`${this.apiUrl}/comparison`);
  }

  // Gets all daily reports
  getDailyReports(): Observable<DailyReport[]> {
    return this.http.get<DailyReport[]>(`${this.apiUrl}/daily-reports`);
  }

  // Gets all numbers for dashboard KPI cards
  getDashboardStats(): Observable<any> {
    return this.http.get<any>(`${this.dashboardUrl}/stats`);
  }
}