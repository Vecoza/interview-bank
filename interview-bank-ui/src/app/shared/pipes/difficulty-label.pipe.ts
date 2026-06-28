import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'difficultyLabel', standalone: true })
export class DifficultyLabelPipe implements PipeTransform {
  private static readonly labels: Record<number, string> = {
    1: 'Easy',
    2: 'Medium',
    3: 'Hard'
  };

  transform(value: number): string {
    return DifficultyLabelPipe.labels[value] ?? 'Unknown';
  }
}
