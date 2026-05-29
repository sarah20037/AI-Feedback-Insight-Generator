import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs/operators';
import { ApiService, FeedbackItem, FeedbackSentiment } from '../../services/api';

interface NegativeFeedbackInsight {
  feedback: FeedbackItem;
  repeatCount: number;
}

@Component({
  selector: 'app-admin-feedback-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-feedback-panel.html',
  styleUrls: ['../feedback-analyzer.css'],
})
export class AdminFeedbackPanelComponent implements OnInit {
  feedbackList: FeedbackItem[] = [];
  isListLoading = false;
  errorMessage = '';
  showAllFeedback = false;

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadFeedback();
  }

  refreshFeedback() {
    this.loadFeedback();
  }

  toggleFeedbackView() {
    this.showAllFeedback = !this.showAllFeedback;
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
    return this.topNegativeFeedbacks[0]?.feedback.feedbackText || 'No repeated negative feedback yet';
  }

  get topRecommendedAction(): string {
    const topNegativeFeedback = this.topNegativeFeedbacks[0];

    if (topNegativeFeedback) return topNegativeFeedback.feedback.recommendedAction;
    if (this.negativeCount > 0) return 'Read recent negative feedback, group similar complaints, and assign one owner for follow-up.';

    return 'Keep monitoring feedback and continue reinforcing what customers already value.';
  }

  get topNegativeFeedbacks(): NegativeFeedbackInsight[] {
    const negativeFeedback = this.feedbackList.filter((feedback) => feedback.sentiment === 'NEGATIVE');
    const repeatCounts = negativeFeedback
      .reduce<Record<string, number>>((counts, feedback) => {
        const issueCategory = this.getNegativeIssueCategory(feedback);
        counts[issueCategory] = (counts[issueCategory] || 0) + 1;
        return counts;
      }, {});

    return negativeFeedback
      .map((feedback) => ({
        feedback,
        repeatCount: repeatCounts[this.getNegativeIssueCategory(feedback)] || 1,
      }))
      .sort((a, b) => {
        return b.repeatCount - a.repeatCount
          || new Date(b.feedback.createdAt).getTime() - new Date(a.feedback.createdAt).getTime();
      })
      .slice(0, 5);
  }

  get reviewPieChartBackground(): string {
    if (this.totalFeedback === 0) return '#e5e7eb';

    const positiveEnd = this.toPiePercent(this.positiveCount);
    const negativeEnd = positiveEnd + this.toPiePercent(this.negativeCount);

    return `conic-gradient(#16a34a 0 ${positiveEnd}%, #dc2626 ${positiveEnd}% ${negativeEnd}%, #c2410c ${negativeEnd}% 100%)`;
  }

  get positivePercent(): number {
    return this.toRoundedPercent(this.positiveCount);
  }

  get negativePercent(): number {
    return this.toRoundedPercent(this.negativeCount);
  }

  get neutralPercent(): number {
    return this.toRoundedPercent(this.neutralCount);
  }

  get hasNegativeFeedback(): boolean {
    return this.feedbackList.some((feedback) => feedback.sentiment === 'NEGATIVE');
  }

  get latestNegativeFeedback(): FeedbackItem[] {
    return this.feedbackList
      .filter((feedback) => feedback.sentiment === 'NEGATIVE')
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      .slice(0, 5);
  }

  get visibleFeedbackList(): FeedbackItem[] {
    const latestFeedback = [...this.feedbackList].sort((a, b) => {
      return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    });

    return this.showAllFeedback ? latestFeedback : latestFeedback.slice(0, 10);
  }

  get hasMoreFeedback(): boolean {
    return this.feedbackList.length > 10;
  }

  private countBySentiment(sentiment: FeedbackSentiment): number {
    return this.feedbackList.filter((feedback) => feedback.sentiment === sentiment).length;
  }

  private getNegativeIssueCategory(feedback: FeedbackItem): string {
    return feedback.issueCategory || 'General negative feedback';
  }

  private toPiePercent(count: number): number {
    return (count / this.totalFeedback) * 100;
  }

  private toRoundedPercent(count: number): number {
    if (this.totalFeedback === 0) return 0;

    return Math.round(this.toPiePercent(count));
  }

  private loadFeedback() {
    this.isListLoading = true;
    this.errorMessage = '';

    this.api.getFeedback().pipe(
      finalize(() => {
        this.isListLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (feedback) => {
        this.feedbackList = Array.isArray(feedback) ? feedback : [];
        this.showAllFeedback = false;
      },
      error: (error) => {
        this.errorMessage = error.name === 'TimeoutError'
          ? 'Feedback records request timed out. Please make sure the backend API is running at http://127.0.0.1:5048.'
          : 'Unable to load feedback records from backend. Please start the backend API and try again.';
        this.cdr.detectChanges();
      },
    });
  }
}
