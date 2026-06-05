import { Component, OnInit, OnDestroy, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService } from '../../core/services/analytics.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy, AfterViewInit {

  @ViewChild('sentimentChart') sentimentChartRef!: ElementRef;
  @ViewChild('topicChart') topicChartRef!: ElementRef;
  @ViewChild('comparisonChart') comparisonChartRef!: ElementRef;

  stats: any = null;
  dailyReports: any[] = [];
  loading = true;
  dataLoaded = false;
  criticalFeedbacks: any[] = [];

  sentimentData: any = null;
  topicData: any = null;
  comparisonData: any = null;

  selectedPeriod = '30';
  periods = [
    { label: 'Last 7 Days', value: '7' },
    { label: 'Last 30 Days', value: '30' },
    { label: 'Last 90 Days', value: '90' },
    { label: 'All Time', value: 'all' }
  ];

  private refreshInterval: any;

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit() {
    this.loadAll();
    this.refreshInterval = setInterval(() => {
      this.loadAll();
    }, 5 * 60 * 1000);
  }

  ngAfterViewInit() {}

  ngOnDestroy() {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  getDateRange(): { from: string, to: string } | null {
    if (this.selectedPeriod === 'all') return null;
    const to = new Date();
    const from = new Date();
    from.setDate(from.getDate() - parseInt(this.selectedPeriod));
    return {
      from: from.toISOString().split('T')[0],
      to: to.toISOString().split('T')[0]
    };
  }

  onPeriodChange() {
  // Destroy charts first
  this.destroyCharts();
  
  // Reset data
  this.sentimentData = null;
  this.topicData = null;
  this.comparisonData = null;
  this.loading = true;

  const range = this.getDateRange();
  const from = range?.from;
  const to = range?.to;

  let loadedCount = 0;
  const total = 3;

  const checkDone = () => {
    loadedCount++;
    if (loadedCount === total) {
      this.loading = false;
      setTimeout(() => {
        this.destroyCharts();
        this.renderCharts();
      }, 300);
    }
  };

  this.analyticsService.getDashboardStats(from, to).subscribe({
  next: (data) => {
    this.stats = data;
    console.log('Stats:', data.totalFeedback, 'feedbacks');
    checkDone();
  },
  error: () => checkDone()
  });

  this.analyticsService.getSentimentSummary(from, to).subscribe({
    next: (data: any) => {
      this.sentimentData = data;
      console.log('Sentiment loaded:', data);
      checkDone();
    },
    error: () => checkDone()
  });

  this.analyticsService.getTopics(from, to).subscribe({
    next: (data: any) => {
      this.topicData = data;
      this.criticalFeedbacks = data.filter((t: any) => t.negativeCount >= 3);
      console.log('Topics loaded:', data);
      checkDone();
    },
    error: () => checkDone()
  });

  this.analyticsService.getComparison(from, to).subscribe({
    next: (data: any) => {
      this.comparisonData = data;
      console.log('Comparison loaded:', data);
      checkDone();
    },
    error: () => checkDone()
  });

  this.analyticsService.getDailyReports().subscribe({
    next: (data: any[]) => { this.dailyReports = data; }
  });
}

  loadAll() {
  this.loading = true;
  const range = this.getDateRange();
  const from = range?.from;
  const to = range?.to;

  let loadedCount = 0;
  const total = 4;

  // TEMP DEBUG — remove after fixing
  console.log('Period:', this.selectedPeriod, 'From:', from, 'To:', to);

  // Reset data before loading
  this.sentimentData = null;
  this.topicData = null;
  this.comparisonData = null;
  this.stats = null;

  const checkDone = () => {
    loadedCount++;
    if (loadedCount === total) {
      this.loading = false;
      this.dataLoaded = true;
      // Small delay to ensure DOM is ready
      setTimeout(() => {
        this.destroyCharts();
        this.renderCharts();
      }, 200);
    }
  };

  this.analyticsService.getDashboardStats(from, to).subscribe({
    next: (data) => { this.stats = data; checkDone(); },
    error: () => checkDone()
  });

  this.analyticsService.getSentimentSummary(from, to).subscribe({
    next: (data: any) => { this.sentimentData = data; checkDone(); },
    error: () => checkDone()
  });

  this.analyticsService.getTopics(from, to).subscribe({
    next: (data: any) => {
      this.topicData = data;
      this.criticalFeedbacks = data.filter((t: any) => t.negativeCount >= 3);
      checkDone();
    },
    error: () => checkDone()
  });

  this.analyticsService.getComparison(from, to).subscribe({
    next: (data: any) => { this.comparisonData = data; checkDone(); },
    error: () => checkDone()
  });

  this.analyticsService.getDailyReports().subscribe({
    next: (data: any[]) => { this.dailyReports = data; }
  });
}

destroyCharts() {
  if (this.sentimentChartRef?.nativeElement) {
    const c1 = Chart.getChart(this.sentimentChartRef.nativeElement);
    if (c1) c1.destroy();
  }
  if (this.topicChartRef?.nativeElement) {
    const c2 = Chart.getChart(this.topicChartRef.nativeElement);
    if (c2) c2.destroy();
  }
  if (this.comparisonChartRef?.nativeElement) {
    const c3 = Chart.getChart(this.comparisonChartRef.nativeElement);
    if (c3) c3.destroy();
  }
}

  exportCSV() {
    if (!this.comparisonData) return;
    const rows = [
      ['Period', 'Total', 'Positive', 'Negative', 'Neutral'],
      ...this.comparisonData.map((d: any) => [
        d.period, d.total, d.positive, d.negative, d.neutral
      ])
    ];
    const csvContent = rows.map(r => r.join(',')).join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `voc-report-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  renderCharts() {
  this.renderSentimentChart();
  this.renderTopicChart();
  this.renderComparisonChart();
}

  renderSentimentChart() {
    if (!this.sentimentData || !this.sentimentChartRef) return;
    const existing = Chart.getChart(this.sentimentChartRef.nativeElement);
    if (existing) existing.destroy();
    const ctx = this.sentimentChartRef.nativeElement.getContext('2d');
    new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['Positive', 'Negative', 'Neutral'],
        datasets: [{
          data: [
            this.sentimentData.positive,
            this.sentimentData.negative,
            this.sentimentData.neutral
          ],
          backgroundColor: ['#1a3a6b', '#2d6abf', '#7eb8f7'],
          borderWidth: 2,
          borderColor: '#ffffff',
          hoverOffset: 6
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#555b6e', padding: 14, font: { size: 12 } }
          }
        },
        cutout: '65%'
      }
    });
  }

  renderTopicChart() {
    if (!this.topicData || !this.topicChartRef) return;
    const existing = Chart.getChart(this.topicChartRef.nativeElement);
    if (existing) existing.destroy();
    const top6 = this.topicData.slice(0, 6);
    const ctx = this.topicChartRef.nativeElement.getContext('2d');
    new Chart(ctx, {
      type: 'bar',
      data: {
        labels: top6.map((t: any) => t.topicName),
        datasets: [
  { label: 'Positive', data: top6.map((t: any) => t.positiveCount), backgroundColor: '#1a3a6b' },
  { label: 'Negative', data: top6.map((t: any) => t.negativeCount), backgroundColor: '#2d6abf' },
  { label: 'Neutral', data: top6.map((t: any) => t.neutralCount), backgroundColor: '#7eb8f7' }
]
      },
      options: {
        responsive: true,
        scales: {
          x: {
            stacked: true,
            ticks: { color: '#555b6e', font: { size: 10 } },
            grid: { color: '#e2e8f0' }
          },
          y: {
            stacked: true,
            ticks: { color: '#555b6e' },
            grid: { color: '#e2e8f0' }
          }
        },
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#555b6e', padding: 14, font: { size: 12 } }
          }
        }
      }
    });
  }

  renderComparisonChart() {
    if (!this.comparisonData || !this.comparisonChartRef) return;
    const existing = Chart.getChart(this.comparisonChartRef.nativeElement);
    if (existing) existing.destroy();
    const ctx = this.comparisonChartRef.nativeElement.getContext('2d');
    new Chart(ctx, {
      type: 'line',
      data: {
        labels: this.comparisonData.map((d: any) => d.period),
        datasets: [
  {
    label: 'Positive',
    data: this.comparisonData.map((d: any) => d.positive),
    borderColor: '#1a3a6b',
    backgroundColor: 'rgba(26,58,107,0.08)',
    tension: 0.4,
    fill: true,
    pointBackgroundColor: '#1a3a6b',
    pointRadius: 4
  },
  {
    label: 'Negative',
    data: this.comparisonData.map((d: any) => d.negative),
    borderColor: '#2d6abf',
    backgroundColor: 'rgba(45,106,191,0.08)',
    tension: 0.4,
    fill: true,
    pointBackgroundColor: '#2d6abf',
    pointRadius: 4
  },
  {
    label: 'Neutral',
    data: this.comparisonData.map((d: any) => d.neutral),
    borderColor: '#7eb8f7',
    backgroundColor: 'rgba(126,184,247,0.08)',
    tension: 0.4,
    fill: true,
    pointBackgroundColor: '#7eb8f7',
    pointRadius: 4
  }
]
      },
      options: {
        responsive: true,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#555b6e', padding: 14, font: { size: 12 } }
          }
        },
        scales: {
          x: { ticks: { color: '#555b6e', font: { size: 10 } }, grid: { color: '#e2e8f0' } },
          y: { ticks: { color: '#555b6e' }, grid: { color: '#e2e8f0' } }
        }
      }
    });
  }
}