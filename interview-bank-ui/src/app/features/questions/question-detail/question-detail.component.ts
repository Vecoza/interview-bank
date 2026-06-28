import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

import { QuestionService, Question } from '../../../core/services/question.service';
import { DifficultyBadgeComponent } from '../../../shared/components/difficulty-badge/difficulty-badge.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-question-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatButtonModule, MatIconModule, MatCardModule,
    MatDividerModule, MatProgressSpinnerModule, MatTooltipModule,
    DifficultyBadgeComponent
  ],
  templateUrl: './question-detail.component.html'
})
export class QuestionDetailComponent implements OnInit {
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private qs     = inject(QuestionService);
  private dialog = inject(MatDialog);

  question = signal<Question | null>(null);
  loading  = signal(true);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.qs.getById(id).subscribe({
      next:  q  => { this.question.set(q); this.loading.set(false); },
      error: () => this.router.navigate(['/questions'])
    });
  }

  togglePracticed() {
    const q = this.question();
    if (!q) return;
    this.qs.togglePracticed(q.id).subscribe(updated => this.question.set(updated));
  }

  confirmDelete() {
    const q = this.question();
    if (!q) return;
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title:   'Delete question',
        message: 'Are you sure you want to delete this question? This cannot be undone.',
        confirm: 'Delete'
      }
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.qs.delete(q.id).subscribe(() => this.router.navigate(['/questions']));
      }
    });
  }
}
