import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [MatIconModule, RouterLink],
  template: `
    <div class="empty-state">
      <mat-icon>{{ icon() }}</mat-icon>
      <h3>{{ title() }}</h3>
      <p>{{ message() }}</p>
      @if (actionLabel() && actionRoute()) {
        <a class="btn-primary" [routerLink]="actionRoute()">{{ actionLabel() }}</a>
      }
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 64px 16px;
      gap: 12px;
      text-align: center;
      mat-icon { font-size: 64px; width: 64px; height: 64px; opacity: 0.25; color: var(--text-muted); }
      h3       { margin: 0; font-size: 1.1rem; color: var(--text-primary); }
      p        { margin: 0; font-size: 14px; color: var(--text-secondary); }
    }
  `]
})
export class EmptyStateComponent {
  icon        = input('inbox');
  title       = input('Nothing here yet');
  message     = input('');
  actionLabel = input('');
  actionRoute = input('');
}
