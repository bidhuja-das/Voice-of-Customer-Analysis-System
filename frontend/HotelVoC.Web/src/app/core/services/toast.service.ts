import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private toasts$ = new BehaviorSubject<Toast[]>([]);
  toasts = this.toasts$.asObservable();
  private counter = 0;

  show(message: string, type: 'success' | 'error' | 'warning' | 'info' = 'info') {
    const id = ++this.counter;
    const toast: Toast = { id, message, type };
    this.toasts$.next([...this.toasts$.value, toast]);

    // Auto remove after 4 seconds
    setTimeout(() => this.remove(id), 4000);
  }

  success(message: string) { this.show(message, 'success'); }
  error(message: string) { this.show(message, 'error'); }
  warning(message: string) { this.show(message, 'warning'); }
  info(message: string) { this.show(message, 'info'); }

  remove(id: number) {
    this.toasts$.next(this.toasts$.value.filter(t => t.id !== id));
  }
}