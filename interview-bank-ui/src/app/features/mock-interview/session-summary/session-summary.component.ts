import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

import { MockInterviewService } from '../../../core/services/mock-interview.service';
import { DifficultyBadgeComponent } from '../../../shared/components/difficulty-badge/difficulty-badge.component';

@Component({
  selector: 'app-session-summary',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatButtonModule, MatCardModule, MatIconModule, MatDividerModule,
    DifficultyBadgeComponent
  ],
  templateUrl: './session-summary.component.html'
})
export class SessionSummaryComponent implements OnInit {
  private router = inject(Router);
  private mis    = inject(MockInterviewService);

  summary = this.mis.sessionSummary;

  ngOnInit() {
    if (!this.summary()) this.router.navigate(['/mock-interview']);
  }

  scorePct(): number {
    const s = this.summary();
    if (!s || s.totalQuestions === 0) return 0;
    return Math.round(((s.gotItCount + s.partialCount * 0.5) / s.totalQuestions) * 100);
  }

  assessmentLabel(val: number): string {
    return ['', 'Got it', 'Partially got it', 'Missed it'][val] ?? '';
  }

  startNew() {
    this.mis.clearSession();
    this.router.navigate(['/mock-interview']);
  }
}
