import { Pipe, PipeTransform } from '@angular/core';
import { Feedback } from '../services/feedback.service';

@Pipe({ name: 'sentimentCount', standalone: true })
export class SentimentCountPipe implements PipeTransform {
  transform(feedbacks: Feedback[], sentiment: string): number {
    return feedbacks.filter(f => f.sentiment === sentiment).length;
  }
}

@Pipe({ name: 'pendingCount', standalone: true })
export class PendingCountPipe implements PipeTransform {
  transform(feedbacks: Feedback[]): number {
    return feedbacks.filter(f => !f.isAnalyzed).length;
  }
}