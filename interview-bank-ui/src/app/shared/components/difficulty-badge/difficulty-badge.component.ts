import { Component, input } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-difficulty-badge',
  standalone: true,
  imports: [MatChipsModule],
  template: `
    <mat-chip [class]="chipClass()" [disableRipple]="true" [disabled]="true">
      {{ label() }}
    </mat-chip>
  `,
  styles: [`
    mat-chip.easy    { --mdc-chip-label-text-color: #166534; background: #dcfce7; }
    mat-chip.medium  { --mdc-chip-label-text-color: #92400e; background: #fef3c7; }
    mat-chip.hard    { --mdc-chip-label-text-color: #991b1b; background: #fee2e2; }
  `]
})
export class DifficultyBadgeComponent {
  difficulty = input.required<number>();

  label()     { return ['', 'Easy', 'Medium', 'Hard'][this.difficulty()] ?? ''; }
  chipClass() { return ['', 'easy', 'medium', 'hard'][this.difficulty()] ?? ''; }
}
