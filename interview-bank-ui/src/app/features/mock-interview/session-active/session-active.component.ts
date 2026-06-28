import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { MockInterviewService, SessionQuestion, SubmitAnswer } from '../../../core/services/mock-interview.service';
import { CountdownTimerComponent } from '../../../shared/components/countdown-timer/countdown-timer.component';
import { DifficultyBadgeComponent } from '../../../shared/components/difficulty-badge/difficulty-badge.component';

@Component({
  selector: 'app-session-active',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatInputModule, MatFormFieldModule,
    MatProgressBarModule, MatCardModule, MatIconModule,
    CountdownTimerComponent, DifficultyBadgeComponent
  ],
  templateUrl: './session-active.component.html'
})
export class SessionActiveComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private mis    = inject(MockInterviewService);

  session        = this.mis.activeSession;
  currentIndex   = signal(0);
  timeRemaining  = signal(0);
  userAnswer     = signal('');
  isRevealed     = signal(false);
  selfAssessment = signal<number | null>(null);
  submitting     = signal(false);

  currentQuestion = computed<SessionQuestion | null>(() => {
    const s = this.session();
    if (!s) return null;
    return s.questions[this.currentIndex()] ?? null;
  });

  progressPct = computed(() => {
    const s = this.session();
    if (!s) return 0;
    return ((this.currentIndex()) / s.questions.length) * 100;
  });

  private answers: SubmitAnswer[] = [];
  private timerSub?: Subscription;
  private questionStartTime = 0;

  ngOnInit() {
    if (!this.session()) {
      this.router.navigate(['/mock-interview']);
      return;
    }
    this.startQuestion();
  }

  ngOnDestroy() {
    this.timerSub?.unsubscribe();
  }

  private startQuestion() {
    this.userAnswer.set('');
    this.isRevealed.set(false);
    this.selfAssessment.set(null);
    this.questionStartTime = Date.now();
    this.timeRemaining.set(this.session()!.timePerQuestion);
    this.startTimer();
  }

  private startTimer() {
    this.timerSub?.unsubscribe();
    this.timerSub = interval(1000).subscribe(() => {
      const next = this.timeRemaining() - 1;
      this.timeRemaining.set(next);
      if (next <= 0) this.revealAnswer();
    });
  }

  revealAnswer() {
    this.timerSub?.unsubscribe();
    this.isRevealed.set(true);
  }

  rate(value: number) {
    this.selfAssessment.set(value);
  }

  next() {
    const q = this.currentQuestion();
    const s = this.session();
    if (!q || !s) return;

    const taken = Math.round((Date.now() - this.questionStartTime) / 1000);

    this.answers.push({
      sessionId:        s.sessionId,
      questionId:       q.id,
      userAnswer:       this.userAnswer() || undefined,
      timeTakenSeconds: Math.min(taken, s.timePerQuestion),
      selfAssessment:   this.selfAssessment() ?? 3
    });

    const nextIndex = this.currentIndex() + 1;

    if (nextIndex >= s.questions.length) {
      this.finishSession();
    } else {
      this.currentIndex.set(nextIndex);
      this.startQuestion();
    }
  }

  private finishSession() {
    this.submitting.set(true);
    this.mis.complete({
      sessionId: this.session()!.sessionId,
      answers:   this.answers
    }).subscribe({
      next: summary => {
        this.mis.setSummary(summary);
        this.router.navigate(['/mock-interview/summary']);
      },
      error: () => {
        this.submitting.set(false);
      }
    });
  }

  readonly assessmentOptions = [
    { value: 1, label: 'Got it',         color: 'primary' },
    { value: 2, label: 'Partially got it', color: 'accent' },
    { value: 3, label: 'Missed it',      color: 'warn' }
  ];
}
