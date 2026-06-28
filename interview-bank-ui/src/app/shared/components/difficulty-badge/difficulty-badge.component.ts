import { Component, input } from '@angular/core';

@Component({
  selector: 'app-difficulty-badge',
  standalone: true,
  imports: [],
  template: `
    <span [class]="'tag ' + tagClass()">{{ label() }}</span>
  `,
  styles: []
})
export class DifficultyBadgeComponent {
  difficulty = input.required<number>();

  label()    { return ['', 'Easy', 'Medium', 'Hard'][this.difficulty()] ?? ''; }
  tagClass() { return ['', 'tag-easy', 'tag-medium', 'tag-hard'][this.difficulty()] ?? ''; }
}
