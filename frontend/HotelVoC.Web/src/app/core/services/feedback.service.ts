import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Feedback {
  feedbackId: number;
  sourceName: string;
  customerIdentifier: string;
  rawText: string;
  submittedAt: string;
  isAnalyzed: boolean;
  sentiment: string | null;
  topic: string | null;
}

export interface IngestFeedback {
  sourceId: number;
  customerIdentifier: string;
  rawText: string;
  submittedAt: string;
}

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private apiUrl = 'http://localhost:5157/api/feedback';

  constructor(private http: HttpClient) {}

  // Gets all feedback — used in feedback list page
  getAll(): Observable<Feedback[]> {
    return this.http.get<Feedback[]>(this.apiUrl);
  }

  // Adds one feedback manually
  ingestOne(feedback: IngestFeedback): Observable<any> {
    return this.http.post(`${this.apiUrl}/ingest`, feedback);
  }

  // Uploads CSV file — sends as multipart form data
  bulkIngest(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/bulk-ingest`, formData);
  }
}