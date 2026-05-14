import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ApiService, FeedbackItem, FeedbackSentiment } from '../services/api';

@Component({
  selector: 'app-feedback-analyzer',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './feedback-analyzer.html',
  styleUrls: ['./feedback-analyzer.css'],
})
export class FeedbackAnalyzerComponent {

  feedbackText: string = '';
  role: string | null = this.getStoredValue('role');
  userName: string = this.getStoredValue('user') || 'User';
  userEmail: string = this.getStoredValue('userEmail') || '';
  successMessage: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;
  isListLoading: boolean = false;
  hasSubmittedFeedback: boolean = false;
  feedbackList: FeedbackItem[] = [];

  constructor(private api: ApiService) {
    this.loadFeedback();
  }

  submitFeedback() {
    const text = this.feedbackText.trim();

    if (!text) {
      this.errorMessage = 'Please write feedback before submitting.';
      return;
    }

    if (!this.userName || !this.userEmail) {
      this.errorMessage = 'Please login again before submitting feedback.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.submitFeedback(this.userName, this.userEmail, text).subscribe({
      next: (response) => {
        this.feedbackList = [response.feedback, ...this.feedbackList];
        this.successMessage = 'Feedback submitted successfully. Thank you for sharing your response.';
        this.hasSubmittedFeedback = true;
        this.feedbackText = '';
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Feedback could not be processed. Please check the backend and AI service.';
        this.isLoading = false;
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
    localStorage.removeItem('userEmail');
    localStorage.removeItem('customerId');
    location.reload();
  }

  get totalFeedback(): number {
    return this.feedbackList.length;
  }

  get positiveCount(): number {
    return this.countBySentiment('POSITIVE');
  }

  get negativeCount(): number {
    return this.countBySentiment('NEGATIVE');
  }

  get neutralCount(): number {
    return this.countBySentiment('NEUTRAL');
  }

  get mostFrequentNegativeIssue(): string {
    const negativeFeedback = this.feedbackList.filter((feedback) => feedback.sentiment === 'NEGATIVE');

    if (negativeFeedback.length === 0) return 'No repeated negative issue yet';

    const issueCounts = negativeFeedback.reduce<Record<string, number>>((counts, feedback) => {
      const issue = feedback.issueCategory || 'General negative feedback';
      counts[issue] = (counts[issue] || 0) + 1;
      return counts;
    }, {});

    return Object.entries(issueCounts).sort((a, b) => b[1] - a[1])[0][0];
  }

  get topRecommendedAction(): string {
    const mostFrequentIssue = this.mostFrequentNegativeIssue;
    const matchedFeedback = this.feedbackList.find((feedback) => feedback.issueCategory === mostFrequentIssue);

    if (matchedFeedback) return matchedFeedback.recommendedAction;
    if (this.negativeCount > 0) return 'Read recent negative feedback, group similar complaints, and assign one owner for follow-up.';

    return 'Keep monitoring feedback and continue reinforcing what customers already value.';
  }

  get negativeFeedbackList(): FeedbackItem[] {
    return this.feedbackList.filter((feedback) => feedback.sentiment === 'NEGATIVE');
  }

  private countBySentiment(sentiment: FeedbackSentiment): number {
    return this.feedbackList.filter((feedback) => feedback.sentiment === sentiment).length;
  }

  private loadFeedback() {
    if (this.role !== 'admin') return;

    this.isListLoading = true;

    this.api.getFeedback().subscribe({
      next: (feedback) => {
        this.feedbackList = feedback;
        this.isListLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load feedback records from backend.';
        this.isListLoading = false;
      },
    });
  }

  private getStoredValue(key: string): string | null {
    if (typeof window === 'undefined') return null;

    return localStorage.getItem(key);
  }
}
