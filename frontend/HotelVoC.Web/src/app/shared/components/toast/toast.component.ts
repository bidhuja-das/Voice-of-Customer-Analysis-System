import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      <div
        *ngFor="let toast of toastService.toasts | async"
        class="toast"
        [ngClass]="'toast-' + toast.type">
        <span class="toast-message">{{ toast.message }}</span>
        <button class="toast-close" (click)="toastService.remove(toast.id)">✕</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 380px;
    }

    .toast {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      padding: 14px 16px;
      border-radius: 10px;
      font-size: 14px;
      font-weight: 500;
      box-shadow: 0 4px 16px rgba(0,0,0,0.12);
      animation: slideIn 0.3s ease;

      &.toast-success {
        background: #f0fdf4;
        border: 1px solid #86efac;
        color: #166534;
      }
      &.toast-error {
        background: #fef2f2;
        border: 1px solid #fca5a5;
        color: #991b1b;
      }
      &.toast-warning {
        background: #fffbeb;
        border: 1px solid #fcd34d;
        color: #92400e;
      }
      &.toast-info {
        background: #eff6ff;
        border: 1px solid #93c5fd;
        color: #1e40af;
      }
    }

    .toast-message { flex: 1; }

    .toast-close {
      background: transparent;
      border: none;
      cursor: pointer;
      font-size: 14px;
      opacity: 0.6;
      padding: 2px 6px;
      border-radius: 4px;
      &:hover { opacity: 1; }
    }

    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class ToastComponent {
  constructor(public toastService: ToastService) {}
}