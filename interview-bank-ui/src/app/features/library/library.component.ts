import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { LibraryService, LibraryQuestion } from '../../core/services/library.service';

@Component({
  selector: 'app-library',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './library.component.html'
})
export class LibraryComponent implements OnInit {
  private svc      = inject(LibraryService);
  private snackBar = inject(MatSnackBar);
  private router   = inject(Router);

  readonly topics = ['JavaScript', 'C#', 'SQL', 'Algorithms'];
  readonly difficulties = [
    { value: 1, label: 'Easy' },
    { value: 2, label: 'Medium' },
    { value: 3, label: 'Hard' }
  ];

  allQuestions  = signal<LibraryQuestion[]>([]);
  loading       = signal(true);
  importing     = signal(false);
  selectedTopic = signal<string | null>(null);
  selectedDiff  = signal<number | null>(null);
  selected      = signal<Set<string>>(new Set());

  filtered = computed(() => {
    let qs = this.allQuestions();
    const t = this.selectedTopic();
    const d = this.selectedDiff();
    if (t) qs = qs.filter(q => q.topicName === t);
    if (d) qs = qs.filter(q => q.difficulty === d);
    return qs;
  });

  selectedCount    = computed(() => this.selected().size);
  selectableCount  = computed(() => this.filtered().filter(q => !q.alreadyImported).length);

  ngOnInit() {
    this.svc.getAll().subscribe({
      next: qs => { this.allQuestions.set(qs); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  setTopic(t: string | null) {
    this.selectedTopic.set(this.selectedTopic() === t ? null : t);
    this.selected.set(new Set());
  }

  setDiff(d: number | null) {
    this.selectedDiff.set(this.selectedDiff() === d ? null : d);
    this.selected.set(new Set());
  }

  toggleSelect(id: string) {
    if (this.allQuestions().find(q => q.id === id)?.alreadyImported) return;
    const s = new Set(this.selected());
    s.has(id) ? s.delete(id) : s.add(id);
    this.selected.set(s);
  }

  toggleAll() {
    const ids = this.filtered().filter(q => !q.alreadyImported).map(q => q.id);
    const allSelected = ids.length > 0 && ids.every(id => this.selected().has(id));
    const s = new Set(this.selected());
    if (allSelected) ids.forEach(id => s.delete(id));
    else             ids.forEach(id => s.add(id));
    this.selected.set(s);
  }

  allFilteredSelected() {
    const ids = this.filtered().filter(q => !q.alreadyImported).map(q => q.id);
    return ids.length > 0 && ids.every(id => this.selected().has(id));
  }

  importSelected() {
    const ids = [...this.selected()];
    if (!ids.length) return;
    this.importing.set(true);
    this.svc.import(ids).subscribe({
      next: res => {
        this.importing.set(false);
        this.selected.set(new Set());
        this.allQuestions.update(list =>
          list.map(q => ids.includes(q.id) ? { ...q, alreadyImported: true } : q)
        );

        const parts: string[] = [];
        if (res.imported > 0)      parts.push(`${res.imported} question${res.imported === 1 ? '' : 's'} added to your bank`);
        if (res.alreadyInBank > 0) parts.push(`${res.alreadyInBank} already in your bank`);
        if (res.unmatchedTopics.length) parts.push(`${res.unmatchedTopics.length} skipped (topic not found)`);

        this.snackBar.open(parts.join(', ') || 'Nothing to import.', 'View', { duration: 5000 })
          .onAction().subscribe(() => this.router.navigate(['/questions']));
      },
      error: () => {
        this.importing.set(false);
        this.snackBar.open('Import failed. Please try again.', 'OK', { duration: 4000 });
      }
    });
  }
}
