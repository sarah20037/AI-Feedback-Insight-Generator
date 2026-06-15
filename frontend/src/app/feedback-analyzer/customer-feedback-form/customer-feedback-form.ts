import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-customer-feedback-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-feedback-form.html',
  styleUrls: ['../feedback-analyzer.css'],
})
export class CustomerFeedbackFormComponent {
  @Input() customerId: number | null = null;
  @Input() userName = 'User';
  @Output() feedbackSaved = new EventEmitter<void>();

  feedbackText = '';
  successMessage = '';
  errorMessage = '';
  isLoading = false;
  hasSubmittedFeedback = false;

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
        this.feedbackSaved.emit();
      },
      error: (error) => {
        this.errorMessage = error.name === 'TimeoutError'
          ? 'Feedback request timed out. Please make sure the backend API is running at http://127.0.0.1:5048.'
          : 'Feedback could not be processed. Please check the backend and database.';
      },
    });
  }

  submitAnotherFeedback() {
    this.hasSubmittedFeedback = false;
    this.successMessage = '';
    this.errorMessage = '';
    this.feedbackText = '';
  }
}
