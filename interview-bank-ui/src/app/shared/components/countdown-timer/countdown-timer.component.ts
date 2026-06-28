import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-countdown-timer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <svg viewBox="0 0 100 100" class="timer-svg">
      <circle cx="50" cy="50" r="45" fill="none" stroke="var(--border, #334155)" stroke-width="8" />
      <circle
        cx="50" cy="50" r="45"
        fill="none"
        [attr.stroke]="isWarning() ? 'var(--danger, #ef4444)' : 'var(--accent, #3b82f6)'"
        stroke-width="8"
        stroke-linecap="round"
        stroke-dasharray="282.7"
        [attr.stroke-dashoffset]="dashOffset()"
        transform="rotate(-90 50 50)"
        style="transition:stroke-dashoffset 0.9s linear;" />
      <text x="50" y="50" text-anchor="middle" dominant-baseline="central"
            [attr.fill]="isWarning() ? 'var(--danger, #ef4444)' : 'var(--text-primary, #f1f5f9)'"
            font-size="22" font-weight="700" font-family="Inter,sans-serif">
        {{ timeRemaining() }}
      </text>
    </svg>
  `,
  styles: [`
    .timer-svg { width: 140px; height: 140px; }
  `]
})
export class CountdownTimerComponent {
  timeRemaining  = input.required<number>();
  totalTime      = input.required<number>();

  isWarning  = computed(() => this.timeRemaining() <= 10);
  dashOffset = computed(() => {
    const pct = this.timeRemaining() / this.totalTime();
    return 282.7 * (1 - Math.max(0, Math.min(1, pct)));
  });
}
