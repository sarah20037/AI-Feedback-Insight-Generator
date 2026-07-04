import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from './login/login';
import { FeedbackAnalyzerComponent } from './feedback-analyzer/feedback-analyzer';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, LoginComponent, FeedbackAnalyzerComponent],
  templateUrl: './app.html',
})
export class AppComponent {
  role: string | null = null;
  user: string = '';
  customerId: number | null = null;
  isFirstTimeUser: boolean = false;

  constructor() {
    if (typeof localStorage === 'undefined') return;

    const savedCustomerId = Number(localStorage.getItem('customerId'));

    if (savedCustomerId) {
      this.role = 'customer';
      this.user = 'User';
      this.customerId = savedCustomerId;
    }
  }

  handleLoginSuccess(event: any) {
    this.role = event.role;
    this.user = event.username;
    this.customerId = event.customerId;
    this.isFirstTimeUser = event.isFirstTimeUser;

    if (typeof localStorage !== 'undefined') {
      if (this.role === 'customer' && this.customerId) {
        localStorage.setItem('customerId', String(this.customerId));
      } else {
        localStorage.removeItem('customerId');
      }
    }
  }

  handleLogout() {
    this.role = null;
    this.user = '';
    this.customerId = null;
    this.isFirstTimeUser = false;

    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('customerId');
    }
  }
}
