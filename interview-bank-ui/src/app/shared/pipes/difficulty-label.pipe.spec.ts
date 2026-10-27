import { DifficultyLabelPipe } from './difficulty-label.pipe';

describe('DifficultyLabelPipe', () => {
  const pipe = new DifficultyLabelPipe();

  it('maps difficulty levels to labels', () => {
    expect(pipe.transform(1)).toBe('Easy');
    expect(pipe.transform(2)).toBe('Medium');
    expect(pipe.transform(3)).toBe('Hard');
  });

  it('falls back for unknown values', () => {
    expect(pipe.transform(7)).toBe('Unknown');
  });
});
