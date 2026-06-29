import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { QuestionService } from '../../../core/services/question.service';
import { TopicService } from '../../../core/services/topic.service';

@Component({
  selector: 'app-question-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './question-form.component.html'
})
export class QuestionFormComponent implements OnInit {
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private qs     = inject(QuestionService);
  private ts     = inject(TopicService);
  private fb     = inject(FormBuilder);

  topics   = this.ts.topics;
  loading  = signal(false);
  saving   = signal(false);
  error    = signal<string | null>(null);
  editId   = signal<string | null>(null);
  isEdit   = signal(false);

  form = this.fb.nonNullable.group({
    text:           ['', [Validators.required, Validators.maxLength(1000)]],
    topicId:        ['', Validators.required],
    difficulty:     [2, Validators.required],
    questionType:   [0],
    expectedAnswer: [''],
    yesNoAnswer:    ['Yes'],
    personalNotes:  [''],
    source:         ['', Validators.maxLength(500)]
  });

  get isYesNo() { return this.form.controls.questionType.value === 1; }

  readonly difficultyOptions = [
    { value: 1, label: 'Easy' },
    { value: 2, label: 'Medium' },
    { value: 3, label: 'Hard' }
  ];

  ngOnInit() {
    this.ts.load().subscribe();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.editId.set(id);
      this.loading.set(true);
      this.qs.getById(id).subscribe({
        next: q => {
          this.form.patchValue({
            text:           q.text,
            topicId:        q.topicId,
            difficulty:     q.difficulty,
            questionType:   q.questionType ?? 0,
            expectedAnswer: q.questionType === 1 ? '' : (q.expectedAnswer ?? ''),
            yesNoAnswer:    q.questionType === 1 ? (q.expectedAnswer ?? 'Yes') : 'Yes',
            personalNotes:  q.personalNotes ?? '',
            source:         q.source ?? ''
          });
          this.loading.set(false);
        },
        error: () => this.router.navigate(['/questions'])
      });
    }
  }

  submit() {
    if (this.form.invalid) return;

    const v = this.form.getRawValue();
    const dto = {
      text:           v.text,
      topicId:        v.topicId,
      difficulty:     v.difficulty,
      questionType:   v.questionType,
      expectedAnswer: v.questionType === 1 ? v.yesNoAnswer : (v.expectedAnswer || undefined),
      personalNotes:  v.personalNotes  || undefined,
      source:         v.source         || undefined
    };

    this.saving.set(true);
    this.error.set(null);

    const req = this.isEdit()
      ? this.qs.update(this.editId()!, dto)
      : this.qs.create(dto);

    req.subscribe({
      next:  q  => this.router.navigate(['/questions', q.id]),
      error: err => {
        this.error.set(err.error?.error ?? 'Failed to save. Please try again.');
        this.saving.set(false);
      }
    });
  }
}
