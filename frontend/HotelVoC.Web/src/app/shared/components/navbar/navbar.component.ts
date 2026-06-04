import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  today = new Date();

  constructor(public authService: AuthService, private router: Router) {}

  getPageTitle(): string {
    const url = this.router.url;
    if (url.includes('dashboard')) return 'Home';
    if (url.includes('feedback')) return 'Feedback Management';
    if (url.includes('insights')) return 'Topics & Insights';
    return 'VoC Analysis';
  }
}