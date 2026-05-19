import { Component, OnInit, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService } from '../../core/services/analytics.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, AfterViewInit {

  @ViewChild('sentimentChart') sentimentChartRef!: ElementRef;
  @ViewChild('topicChart') topicChartRef!: ElementRef;
  @ViewChild('comparisonChart') comparisonChartRef!: ElementRef;

  stats: any = null;
  dailyReports: any[] = [];
  loading = true;

  sentimentData: any = null;
  topicData: any = null;
  comparisonData: any = null;

  dataLoaded = false;

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit() {
    this.loadAll();
  }

  ngAfterViewInit() {}

  loadAll() {
    this.loading = true;
    let loadedCount = 0;
    const total = 4;

    const checkDone = () => {
      loadedCount++;
      if (loadedCount === total) {
        this.loading = false;
        this.dataLoaded = true;
        setTimeout(() => this.renderCharts(), 100);
      }
    };

    this.analyticsService.getDashboardStats().subscribe({
      next: (data) => { this.stats = data; checkDone(); },
      error: () => checkDone()
    });

    this.analyticsService.getSentimentSummary().subscribe({
      next: (data: any) => { this.sentimentData = data; checkDone(); },
      error: () => checkDone()
    });

    this.analyticsService.getTopics().subscribe({
      next: (data: any) => { this.topicData = data; checkDone(); },
      error: () => checkDone()
    });

    this.analyticsService.getComparison().subscribe({
      next: (data: any) => { this.comparisonData = data; checkDone(); },
      error: () => checkDone()
    });

    this.analyticsService.getDailyReports().subscribe({
      next: (data: any[]) => { this.dailyReports = data; }
    });
  }

  renderCharts() {
    this.renderSentimentChart();
    this.renderTopicChart();
    this.renderComparisonChart();
  }

  renderSentimentChart() {
    if (!this.sentimentData || !this.sentimentChartRef) return;
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
          backgroundColor: ['#2ecc71', '#e74c3c', '#f39c12'],
          borderWidth: 0,
          hoverOffset: 8
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#8892a4', padding: 16, font: { size: 13 } }
          }
        },
        cutout: '65%'
      }
    });
  }

  renderTopicChart() {
    if (!this.topicData || !this.topicChartRef) return;
    const top8 = this.topicData.slice(0, 8);
    const ctx = this.topicChartRef.nativeElement.getContext('2d');
    new Chart(ctx, {
      type: 'bar',
      data: {
        labels: top8.map((t: any) => t.topicName),
        datasets: [
          {
            label: 'Positive',
            data: top8.map((t: any) => t.positiveCount),
            backgroundColor: '#2ecc71'
          },
          {
            label: 'Negative',
            data: top8.map((t: any) => t.negativeCount),
            backgroundColor: '#e74c3c'
          },
          {
            label: 'Neutral',
            data: top8.map((t: any) => t.neutralCount),
            backgroundColor: '#f39c12'
          }
        ]
      },
      options: {
        responsive: true,
        scales: {
          x: {
            stacked: true,
            ticks: { color: '#8892a4', font: { size: 11 } },
            grid: { color: '#2d3148' }
          },
          y: {
            stacked: true,
            ticks: { color: '#8892a4' },
            grid: { color: '#2d3148' }
          }
        },
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#8892a4', padding: 16 }
          }
        }
      }
    });
  }

  renderComparisonChart() {
    if (!this.comparisonData || !this.comparisonChartRef) return;
    const ctx = this.comparisonChartRef.nativeElement.getContext('2d');
    new Chart(ctx, {
      type: 'line',
      data: {
        labels: this.comparisonData.map((d: any) => d.period),
        datasets: [
          {
            label: 'Positive',
            data: this.comparisonData.map((d: any) => d.positive),
            borderColor: '#2ecc71',
            backgroundColor: 'rgba(46,204,113,0.1)',
            tension: 0.4,
            fill: true,
            pointBackgroundColor: '#2ecc71',
            pointRadius: 5
          },
          {
            label: 'Negative',
            data: this.comparisonData.map((d: any) => d.negative),
            borderColor: '#e74c3c',
            backgroundColor: 'rgba(231,76,60,0.1)',
            tension: 0.4,
            fill: true,
            pointBackgroundColor: '#e74c3c',
            pointRadius: 5
          },
          {
            label: 'Neutral',
            data: this.comparisonData.map((d: any) => d.neutral),
            borderColor: '#f39c12',
            backgroundColor: 'rgba(243,156,18,0.1)',
            tension: 0.4,
            fill: true,
            pointBackgroundColor: '#f39c12',
            pointRadius: 5
          }
        ]
      },
      options: {
        responsive: true,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#8892a4', padding: 16 }
          }
        },
        scales: {
          x: {
            ticks: { color: '#8892a4' },
            grid: { color: '#2d3148' }
          },
          y: {
            ticks: { color: '#8892a4' },
            grid: { color: '#2d3148' }
          }
        }
      }
    });
  }
}