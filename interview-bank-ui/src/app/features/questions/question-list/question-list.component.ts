import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';

import { QuestionService, Question, QuestionFilter } from '../../../core/services/question.service';
import { TopicService } from '../../../core/services/topic.service';
import { DifficultyBadgeComponent } from '../../../shared/components/difficulty-badge/difficulty-badge.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-question-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    MatInputModule, MatFormFieldModule, MatChipsModule,
    MatButtonModule, MatIconModule, MatProgressBarModule,
    MatPaginatorModule, MatTooltipModule,
    DifficultyBadgeComponent, EmptyStateComponent
  ],
  templateUrl: './question-list.component.html'
})
export class QuestionListComponent implements OnInit {
  private route   = inject(ActivatedRoute);
  private router  = inject(Router);
  private qs      = inject(QuestionService);
  private ts      = inject(TopicService);
  private dialog  = inject(MatDialog);
  private search$ = new Subject<string>();

  topics    = this.ts.topics;
  questions = signal<Question[]>([]);
  total     = signal(0);
  loading   = signal(false);

  searchText       = signal('');
  selectedTopicIds = signal<string[]>([]);
  selectedDiffs    = signal<number[]>([]);
  practicedFilter  = signal<'all' | 'yes' | 'no'>('all');
  page             = signal(1);
  pageSize         = signal(20);

  ngOnInit() {
    this.ts.load().subscribe();

    this.route.queryParams.pipe(takeUntilDestroyed()).subscribe(params => {
      this.searchText.set(params['search'] ?? '');
      this.selectedTopicIds.set(params['topics'] ? params['topics'].split(',') : []);
      this.selectedDiffs.set(params['difficulties'] ? params['difficulties'].split(',').map(Number) : []);
      this.practicedFilter.set(params['practiced'] ?? 'all');
      this.page.set(Number(params['page'] ?? 1));
      this.fetchQuestions();
    });

    this.search$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed()
    ).subscribe(text => {
      this.searchText.set(text);
      this.pushToUrl({ search: text || undefined, page: 1 });
    });
  }

  onSearchInput(value: string) { this.search$.next(value); }

  toggleTopic(id: string) {
    const current = this.selectedTopicIds();
    const updated = current.includes(id)
      ? current.filter(t => t !== id)
      : [...current, id];
    this.pushToUrl({ topics: updated.join(',') || undefined, page: 1 });
  }

  toggleDifficulty(d: number) {
    const current = this.selectedDiffs();
    const updated = current.includes(d)
      ? current.filter(x => x !== d)
      : [...current, d];
    this.pushToUrl({ difficulties: updated.join(',') || undefined, page: 1 });
  }

  setPracticed(val: 'all' | 'yes' | 'no') {
    this.pushToUrl({ practiced: val === 'all' ? undefined : val, page: 1 });
  }

  onPage(e: PageEvent) {
    this.pushToUrl({ page: e.pageIndex + 1, pageSize: e.pageSize });
  }

  clearFilters() {
    this.router.navigate([], { queryParams: {} });
  }

  confirmDelete(q: Question) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete question',
        message: `Delete "${q.text.slice(0, 60)}..."? This cannot be undone.`,
        confirm: 'Delete'
      }
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.qs.delete(q.id).subscribe(() =>
          this.questions.update(list => list.filter(x => x.id !== q.id))
        );
      }
    });
  }

  togglePracticed(q: Question) {
    this.qs.togglePracticed(q.id).subscribe(updated =>
      this.questions.update(list => list.map(x => x.id === updated.id ? updated : x))
    );
  }

  private fetchQuestions() {
    const filter: QuestionFilter = {
      search:       this.searchText() || undefined,
      topicIds:     this.selectedTopicIds().length ? this.selectedTopicIds() : undefined,
      difficulties: this.selectedDiffs().length ? this.selectedDiffs() : undefined,
      isPracticed:  this.practicedFilter() === 'all' ? null
                  : this.practicedFilter() === 'yes',
      page:         this.page(),
      pageSize:     this.pageSize()
    };
    this.loading.set(true);
    this.qs.getAll(filter).subscribe(res => {
      this.questions.set(res.questions);
      this.total.set(res.total);
      this.loading.set(false);
    });
  }

  private pushToUrl(patch: Record<string, string | number | undefined>) {
    const current = this.route.snapshot.queryParams;
    const next = { ...current, ...patch };
    // Remove undefined keys
    Object.keys(next).forEach(k => next[k] === undefined && delete next[k]);
    this.router.navigate([], { queryParams: next });
  }

  readonly difficulties = [
    { value: 1, label: 'Easy' },
    { value: 2, label: 'Medium' },
    { value: 3, label: 'Hard' }
  ];
}
