import { Component, OnInit, signal, computed, inject, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

import { DashboardService, TopicBreakdown } from '../../core/services/dashboard.service';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatProgressBarModule, MatButtonModule,
    MatIconModule, MatDividerModule, MatProgressSpinnerModule,
    EmptyStateComponent
  ],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit, AfterViewInit {
  @ViewChild('difficultyChart') difficultyChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('topicChart')      topicChartRef!: ElementRef<HTMLCanvasElement>;

  private ds = inject(DashboardService);

  stats   = this.ds.stats;
  loading = this.ds.loading;

  practicedPct = computed(() => {
    const s = this.stats();
    if (!s || s.totalQuestions === 0) return 0;
    return Math.round((s.practicedCount / s.totalQuestions) * 100);
  });

  private diffChart?: Chart;
  private topicChart?: Chart;

  ngOnInit() {
    this.ds.load().subscribe(() => this.renderCharts());
  }

  ngAfterViewInit() {
    if (this.stats()) this.renderCharts();
  }

  private renderCharts() {
    const s = this.stats();
    if (!s) return;

    setTimeout(() => {
      this.renderDifficultyChart(s);
      this.renderTopicChart(s.byTopic.slice(0, 10));
    }, 0);
  }

  private renderDifficultyChart(s: ReturnType<typeof this.stats>) {
    if (!s || !this.difficultyChartRef) return;

    this.diffChart?.destroy();

    const labels = Object.keys(s.byDifficulty);
    const totals    = labels.map(l => s.byDifficulty[l].total);
    const practiced = labels.map(l => s.byDifficulty[l].practiced);

    const cfg: ChartConfiguration = {
      type: 'bar',
      data: {
        labels,
        datasets: [
          { label: 'Total',     data: totals,    backgroundColor: '#cbd5e1' },
          { label: 'Practiced', data: practiced,  backgroundColor: '#3b82f6' }
        ]
      },
      options: {
        responsive: true,
        plugins: { legend: { position: 'top' } },
        scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
      }
    };

    this.diffChart = new Chart(this.difficultyChartRef.nativeElement, cfg);
  }

  private renderTopicChart(topics: TopicBreakdown[]) {
    if (!this.topicChartRef) return;

    this.topicChart?.destroy();

    const cfg: ChartConfiguration = {
      type: 'bar',
      data: {
        labels: topics.map(t => t.topicName),
        datasets: [
          { label: 'Total',     data: topics.map(t => t.total),    backgroundColor: '#cbd5e1', borderRadius: 4 },
          { label: 'Practiced', data: topics.map(t => t.practiced), backgroundColor: '#3b82f6', borderRadius: 4 }
        ]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        plugins: { legend: { position: 'top' } },
        scales: { x: { beginAtZero: true, ticks: { stepSize: 1 } } }
      }
    };

    this.topicChart = new Chart(this.topicChartRef.nativeElement, cfg);
  }

  sessionScore(session: { gotItCount: number; partialCount: number; totalQuestions: number }): number {
    if (session.totalQuestions === 0) return 0;
    return Math.round(((session.gotItCount + session.partialCount * 0.5) / session.totalQuestions) * 100);
  }

  missedPct(ratio: number): string {
    return `${Math.round(ratio * 100)}%`;
  }
}
