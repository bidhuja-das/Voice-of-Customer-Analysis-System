import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5157/api/auth';

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('email', response.email);
        localStorage.setItem('role', response.role);
      })
    );
  }

  
  logout(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  
  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  
  getRole(): string | null {
    return localStorage.getItem('role');
  }

  
  getEmail(): string | null {
    return localStorage.getItem('email');
  }
}