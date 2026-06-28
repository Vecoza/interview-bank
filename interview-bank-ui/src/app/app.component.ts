import { Component, inject, computed } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, MatIconModule],
  templateUrl: './app.component.html'
})
export class AppComponent {
  private router = inject(Router);
  private auth   = inject(AuthService);

  isAuthenticated = this.auth.isAuthenticated;

  isFullScreen = computed(() =>
    this.router.url.startsWith('/mock-interview/active')
  );

  logout() {
    this.auth.logout();
  }

  readonly navItems = [
    { path: '/questions',      icon: 'grid_view',    label: 'Questions' },
    { path: '/library',        icon: 'auto_stories', label: 'Library' },
    { path: '/mock-interview', icon: 'psychology',   label: 'Mock Interview' },
    { path: '/dashboard',      icon: 'insights',     label: 'Dashboard' },
  ];
}
