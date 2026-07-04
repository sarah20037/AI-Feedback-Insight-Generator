import { Component, ChangeDetectorRef, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ApiService } from '../services/api';
import { AdminFeedbackPanelComponent } from './admin-feedback-panel/admin-feedback-panel';
import { CustomerHistoryComponent } from './customer-history/customer-history';

@Component({
  selector: 'app-feedback-analyzer',
  standalone: true,
  imports: [FormsModule, CommonModule, AdminFeedbackPanelComponent, CustomerHistoryComponent],
  templateUrl: './feedback-analyzer.html',
  styleUrls: ['./feedback-analyzer.css'],
})
export class FeedbackAnalyzerComponent {

  feedbackText: string = '';
  successMessage: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;
  hasSubmittedFeedback: boolean = false;
  historyRefreshKey: number = 0;

  @Input() role: string | null = null;
  @Input() userName: string = 'User';
  @Input() customerId: number | null = null;
  @Input() isFirstTimeUser: boolean = false;

  @Output() logoutEvent = new EventEmitter<void>();

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  submitFeedback() {
    const text = this.feedbackText.trim();

    if (!text) {
      this.errorMessage = 'Please write feedback before submitting.';
      return;
    }

    if (text.length < 3) {
      this.errorMessage = 'Feedback must be at least 3 characters.';
      return;
    }

    if (text.length > 1000) {
      this.errorMessage = 'Feedback must be 1000 characters or less.';
      return;
    }

    if (!this.customerId) {
      this.errorMessage = 'Please login again before submitting feedback.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.submitFeedback(this.customerId, text).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Feedback submitted successfully. Thank you for sharing your response.';
        this.hasSubmittedFeedback = true;
        this.feedbackText = '';
        this.historyRefreshKey++;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoading = false;
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
    this.logoutEvent.emit();
  }
}
