import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AnalyticsService } from '../../../core/services/analytics.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {

  criticalCount = 0;

  constructor(
    public authService: AuthService,
    private router: Router,
    private analyticsService: AnalyticsService
  ) {}

  ngOnInit() {
    this.loadCriticalCount();
  }

  loadCriticalCount() {
    if (this.authService.getRole() === 'Executive') {
      this.analyticsService.getTopics().subscribe({
        next: (data: any[]) => {
          this.criticalCount = data.filter(t => t.negativeCount >= 3).length;
        },
        error: () => {}
      });
    }
  }

  isActive(path: string): boolean {
    return this.router.url.includes(path);
  }

  logout() {
    this.authService.logout();
  }
}