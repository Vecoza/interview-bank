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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { MockInterviewService, SessionQuestion, SubmitAnswer, AiEvaluation } from '../../../core/services/mock-interview.service';
import { CountdownTimerComponent } from '../../../shared/components/countdown-timer/countdown-timer.component';
import { DifficultyBadgeComponent } from '../../../shared/components/difficulty-badge/difficulty-badge.component';

@Component({
  selector: 'app-session-active',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatInputModule, MatFormFieldModule,
    MatProgressBarModule, MatCardModule, MatIconModule, MatProgressSpinnerModule,
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

  yesNoAnswer    = signal<'Yes' | 'No' | null>(null);
  yesNoCorrect   = signal<boolean | null>(null);

  aiEvaluating   = signal(false);
  aiResult       = signal<AiEvaluation | null>(null);
  aiError        = signal<string | null>(null);

  currentQuestion = computed<SessionQuestion | null>(() => {
    const s = this.session();
    if (!s) return null;
    return s.questions[this.currentIndex()] ?? null;
  });

  isYesNo = computed(() => this.currentQuestion()?.questionType === 1);

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
    this.yesNoAnswer.set(null);
    this.yesNoCorrect.set(null);
    this.aiResult.set(null);
    this.aiError.set(null);
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
    if (this.isYesNo() && this.yesNoAnswer() === null) {
      this.yesNoCorrect.set(false);
      this.selfAssessment.set(3);
    }
    this.isRevealed.set(true);
  }

  answerYesNo(choice: 'Yes' | 'No') {
    if (this.isRevealed()) return;
    this.timerSub?.unsubscribe();
    const correct = choice === this.currentQuestion()?.correctAnswer;
    this.yesNoAnswer.set(choice);
    this.yesNoCorrect.set(correct);
    this.selfAssessment.set(correct ? 1 : 3);
    this.userAnswer.set(choice);
    this.isRevealed.set(true);
  }

  rate(value: number) {
    this.selfAssessment.set(value);
  }

  checkWithAi() {
    const q = this.currentQuestion();
    if (!q || !q.text || !this.userAnswer()) return;

    const expectedAnswer = (this.session()?.questions ?? []).length > 0
      ? this.getExpectedAnswerForAi(q.id)
      : null;

    if (!expectedAnswer) {
      this.aiError.set('No model answer available to compare against.');
      return;
    }

    this.aiEvaluating.set(true);
    this.aiResult.set(null);
    this.aiError.set(null);

    this.mis.evaluateAnswer(q.text, expectedAnswer, this.userAnswer()).subscribe({
      next: result => {
        this.aiResult.set(result);
        this.aiEvaluating.set(false);
      },
      error: () => {
        this.aiError.set('AI evaluation failed. Make sure Anthropic:ApiKey is set in the backend.');
        this.aiEvaluating.set(false);
      }
    });
  }

  private getExpectedAnswerForAi(questionId: string): string | null {
    if (this.mis.isLocalSession()) {
      return this.mis.getLocalExpectedAnswer(questionId);
    }
    return null;
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

    if (this.mis.isLocalSession()) {
      const summary = this.mis.completeLocal(this.answers);
      this.mis.setSummary(summary);
      this.router.navigate(['/mock-interview/summary']);
      return;
    }

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
    { value: 1, label: 'Got it',           color: 'primary' },
    { value: 2, label: 'Partially got it', color: 'accent'  },
    { value: 3, label: 'Missed it',        color: 'warn'    }
  ];
}
