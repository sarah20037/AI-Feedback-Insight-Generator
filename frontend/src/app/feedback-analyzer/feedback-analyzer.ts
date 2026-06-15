import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs/operators';
import { ApiService } from '../services/api';
import { AdminFeedbackPanelComponent } from './admin-feedback-panel/admin-feedback-panel';

@Component({
  selector: 'app-feedback-analyzer',
  standalone: true,
  imports: [FormsModule, CommonModule, AdminFeedbackPanelComponent],
  templateUrl: './feedback-analyzer.html',
  styleUrls: ['./feedback-analyzer.css'],
})
export class FeedbackAnalyzerComponent {

  feedbackText: string = '';
  role: string | null = this.getStoredValue('role');
  userName: string = this.getStoredValue('user') || 'User';
  customerId: number | null = this.getStoredNumber('customerId');
  isFirstTimeUser: boolean = this.getStoredValue('isFirstTimeUser') === 'true';
  successMessage: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;
  hasSubmittedFeedback: boolean = false;

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  submitFeedback() {
    const text = this.feedbackText.trim();

    if (!text) {
      this.errorMessage = 'Please write feedback before submitting.';
      return;
    }

    if (!this.customerId) {
      this.errorMessage = 'Please login again before submitting feedback.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.submitFeedback(this.customerId, text).pipe(
      finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: () => {
        this.successMessage = 'Feedback submitted successfully. Thank you for sharing your response.';
        this.hasSubmittedFeedback = true;
        this.feedbackText = '';
      },
      error: (error) => {
        this.errorMessage = error.name === 'TimeoutError'
          ? 'Feedback request timed out. Please make sure the backend API is running at http://127.0.0.1:5048.'
          : 'Feedback could not be processed. Please check the backend and database.';
        this.cdr.detectChanges();
      },
    });
  }

  submitAnotherFeedback() {
    this.hasSubmittedFeedback = false;
    this.successMessage = '';
    this.errorMessage = '';
    this.feedbackText = '';
  }

  logout() {
    if (typeof window === 'undefined') return;

    localStorage.removeItem('role');
    localStorage.removeItem('user');
    localStorage.removeItem('customerId');
    localStorage.removeItem('isFirstTimeUser');
    location.reload();
  }

  private getStoredValue(key: string): string | null {
    if (typeof window === 'undefined') return null;

    return localStorage.getItem(key);
  }

  private getStoredNumber(key: string): number | null {
    const value = this.getStoredValue(key);
    if (!value) return null;

    const parsedValue = Number(value);
    return Number.isFinite(parsedValue) && parsedValue > 0 ? parsedValue : null;
  }
}
