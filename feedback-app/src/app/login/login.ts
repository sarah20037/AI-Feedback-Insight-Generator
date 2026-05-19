import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../services/api';

type LoginMode = 'login' | 'register' | 'admin';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  imports: [CommonModule, FormsModule]
})
export class LoginComponent {

  name: string = '';
  email: string = '';
  password: string = '';
  mode: LoginMode = 'login';
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(private api: ApiService) {}

  setMode(mode: LoginMode) {
    this.mode = mode;
    this.name = '';
    this.email = '';
    this.password = '';
    this.errorMessage = '';
  }

  login() {
    this.errorMessage = '';
    const name = this.name.trim();
    const loginId = this.email.trim().toLowerCase();

    if (this.mode === 'register' && !name) {
      this.errorMessage = 'Please enter your name.';
      return;
    }

    if (!loginId || !this.password) {
      this.errorMessage = this.mode === 'admin'
        ? 'Please enter admin username and password.'
        : 'Please enter email and password.';
      return;
    }

    if (this.mode !== 'admin' && !this.isValidEmail(loginId)) {
      this.errorMessage = 'Please enter a valid email address.';
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters.';
      return;
    }

    this.isLoading = true;

    if (this.mode === 'register') {
      this.api.register(name, loginId, this.password).subscribe({
        next: (response) => this.startSession(response.fullName || name, 'customer', response.customerId),
        error: () => this.showError('Registration failed. The email may already be registered.'),
      });
    } else if (this.mode === 'admin') {
      this.api.adminLogin(loginId, this.password).subscribe({
        next: (response) => this.startSession(response.username || 'Admin', 'admin'),
        error: () => this.showError('Admin username or password is incorrect.'),
      });
    } else {
      this.api.login(loginId, this.password).subscribe({
        next: (response) => this.startSession(response.fullName || response.username, 'customer', response.customerId),
        error: () => this.showError('Email or password is incorrect.'),
      });
    }
  }

  private startSession(username: string, role: 'customer' | 'admin', customerId?: number) {
    if (typeof window === 'undefined') return;

    localStorage.setItem('role', role);
    localStorage.setItem('user', username);
    if (customerId) localStorage.setItem('customerId', customerId.toString());

    location.reload();
  }

  private showError(message: string) {
    this.isLoading = false;
    this.errorMessage = message;
  }

  private isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }
}
