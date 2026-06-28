import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { MockInterviewService } from '../../../core/services/mock-interview.service';
import { TopicService } from '../../../core/services/topic.service';

@Component({
  selector: 'app-session-setup',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatSelectModule, MatButtonModule,
    MatChipsModule, MatProgressSpinnerModule, MatCardModule, MatIconModule
  ],
  templateUrl: './session-setup.component.html'
})
export class SessionSetupComponent implements OnInit {
  private router  = inject(Router);
  private mis     = inject(MockInterviewService);
  private ts      = inject(TopicService);
  private fb      = inject(FormBuilder);

  topics          = this.ts.topics;
  starting        = signal(false);
  error           = signal<string | null>(null);
  selectedTopics  = signal<string[]>([]);
  selectedDiffs   = signal<number[]>([]);
  dueCount        = signal(0);

  form = this.fb.nonNullable.group({
    questionCount:   [10,  Validators.required],
    strategy:        [0,   Validators.required],
    timePerQuestion: [60,  Validators.required]
  });

  readonly questionCountOptions  = [5, 10, 15, 20];
  readonly timeOptions           = [30, 60, 90, 120, 180];
  readonly strategyOptions = [
    { value: 0, label: 'Random' },
    { value: 1, label: 'Least recently practiced' },
    { value: 2, label: 'Hardest first' },
    { value: 3, label: 'Due for review (spaced repetition)' }
  ];
  readonly difficulties = [
    { value: 1, label: 'Easy' },
    { value: 2, label: 'Medium' },
    { value: 3, label: 'Hard' }
  ];

  ngOnInit() {
    this.ts.load().subscribe();
    this.mis.getDueCount().subscribe(r => this.dueCount.set(r.count));
  }

  startDueReview() {
    this.form.controls.strategy.setValue(3);
    this.start();
  }

  toggleTopic(id: string) {
    const curr = this.selectedTopics();
    this.selectedTopics.set(
      curr.includes(id) ? curr.filter(t => t !== id) : [...curr, id]
    );
  }

  toggleDifficulty(val: number) {
    const curr = this.selectedDiffs();
    this.selectedDiffs.set(
      curr.includes(val) ? curr.filter(d => d !== val) : [...curr, val]
    );
  }

  start() {
    if (this.form.invalid) return;

    const { questionCount, strategy, timePerQuestion } = this.form.getRawValue();

    this.starting.set(true);
    this.error.set(null);

    this.mis.start({
      questionCount,
      strategy,
      timePerQuestion,
      topicIds:     this.selectedTopics().length ? this.selectedTopics() : undefined,
      difficulties: this.selectedDiffs().length  ? this.selectedDiffs()  : undefined
    }).subscribe({
      next: session => {
        this.mis.setActiveSession(session);
        this.router.navigate(['/mock-interview/active']);
      },
      error: err => {
        this.error.set(err.error?.error ?? 'Could not start session. Try different filters.');
        this.starting.set(false);
      }
    });
  }
}
