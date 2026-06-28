import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirm  = control.get('confirmPassword')?.value;
  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  form = this.fb.nonNullable.group(
    {
      email:           ['', [Validators.required, Validators.email]],
      password:        ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    },
    { validators: passwordsMatch }
  );

  loading      = signal(false);
  error        = signal<string | null>(null);
  showPassword = signal(false);
  showConfirm  = signal(false);

  constructor(
    private fb:      FormBuilder,
    private auth:    AuthService,
    private router:  Router,
    private snackBar: MatSnackBar
  ) {}

  submit(): void {
    if (this.form.invalid) return;

    const { email, password } = this.form.getRawValue();
    this.loading.set(true);
    this.error.set(null);

    this.auth.register(email, password).subscribe({
      next: () => {
        this.snackBar.open('Account created! Please sign in.', 'OK', { duration: 4000 });
        this.router.navigate(['/login']);
      },
      error: err => {
        const msgs: string[] = err.error?.errors ?? [];
        this.error.set(msgs.length ? msgs.join(' ') : 'Registration failed. Please try again.');
        this.loading.set(false);
      }
    });
  }
}
