import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  const router = inject(Router);
  const toast = inject(ToastService);

  const cloned = token ? req.clone({
    headers: req.headers.set('Authorization', `Bearer ${token}`)
  }) : req;

  return next(cloned).pipe(
    catchError((error: HttpErrorResponse) => {
      switch (error.status) {
        case 0:
          toast.error('Cannot connect to server. Please check your connection.');
          break;
        case 400:
          const msg400 = error.error?.error || 'Invalid request. Please check your input.';
          toast.error(msg400);
          break;
        case 401:
          toast.warning('Session expired. Please login again.');
          localStorage.clear();
          router.navigate(['/login']);
          break;
        case 403:
          toast.error('You do not have permission to perform this action.');
          break;
        case 404:
          toast.error('The requested resource was not found.');
          break;
        case 500:
          toast.error('Server error. Please try again later.');
          break;
        default:
          toast.error('An unexpected error occurred.');
      }
      return throwError(() => error);
    })
  );
};