import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InsightService, Insight } from '../../core/services/insight.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-topics-insights',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './topics-insights.component.html',
  styleUrls: ['./topics-insights.component.scss']
})
export class TopicsInsightsComponent implements OnInit {
  insights: Insight[] = [];
  loading = false;
  generating = false;
  generateMessage = '';
  generateError = '';

  constructor(
    private insightService: InsightService,
    public authService: AuthService,
    private toast: ToastService
  ) {}

  ngOnInit() {
    this.loadInsights();
  }

  loadInsights() {
    this.loading = true;
    this.insightService.getAll().subscribe({
      next: (data) => {
        this.insights = data;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  generateInsights() {
  this.generating = true;

  this.insightService.generate().subscribe({
    next: () => {
      this.generating = false;
      this.toast.success('Insights generated successfully!');
      this.loadInsights();
    },
    error: () => {
      this.generating = false;
      this.toast.error('Failed to generate insights. Make sure Ollama is running.');
    }
  });
}

  // Filter insights by urgency level
  getByUrgency(level: string): Insight[] {
    return this.insights.filter(i => i.urgencyLevel === level);
  }

  getUrgencyIcon(level: string): string {
    const icons: any = {
      'Critical': '🔴',
      'High': '🟠',
      'Medium': '🟡',
      'Low': '🟢'
    };
    return icons[level] || '⚪';
  }

  getUrgencyClass(level: string): string {
    return 'badge-' + level?.toLowerCase();
  }

  scrollTo(id: string) {
  const el = document.getElementById(id);
  if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
}
}