import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CountdownTimerComponent } from './countdown-timer.component';

describe('CountdownTimerComponent', () => {
  let fixture: ComponentFixture<CountdownTimerComponent>;

  function render(remaining: number, total: number) {
    fixture.componentRef.setInput('timeRemaining', remaining);
    fixture.componentRef.setInput('totalTime', total);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(() => {
    fixture = TestBed.createComponent(CountdownTimerComponent);
  });

  it('shows the remaining seconds', () => {
    render(42, 60);
    expect(fixture.nativeElement.querySelector('text').textContent.trim()).toBe('42');
  });

  it('switches to warning state in the last ten seconds', () => {
    expect(render(30, 60).isWarning()).toBeFalse();
    expect(render(10, 60).isWarning()).toBeTrue();
  });

  it('draws the ring proportionally and clamps out-of-range values', () => {
    expect(render(60, 60).dashOffset()).toBeCloseTo(0);
    expect(render(30, 60).dashOffset()).toBeCloseTo(282.7 / 2);
    expect(render(-5, 60).dashOffset()).toBeCloseTo(282.7);
  });
});
