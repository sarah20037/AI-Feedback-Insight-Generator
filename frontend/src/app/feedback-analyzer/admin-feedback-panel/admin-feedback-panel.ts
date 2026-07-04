import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, FeedbackItem } from '../../services/api';

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
  topNegativeFeedbacks: NegativeFeedbackInsight[] = [];
  totalFeedback = 0;
  positiveCount = 0;
  negativeCount = 0;
  neutralCount = 0;
  isListLoading = false;
  isNextPageLoading = false;
  errorMessage = '';
  hasLoadedOverview = false;
  currentPage = 1;
  readonly pageSize = 10;

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadOverview();
  }

  refreshFeedback() {
    this.currentPage = 1;
    this.loadOverview();
  }

  loadNextFeedbackPage() {
    if (this.isNextPageLoading || !this.hasMoreFeedback) {
      return;
    }

    this.isNextPageLoading = true;
    this.errorMessage = '';

    this.api.getFeedbackPage(this.currentPage + 1, this.pageSize).subscribe({
      next: (result) => {
        this.isNextPageLoading = false;
        this.feedbackList = [
          ...this.feedbackList,
          ...this.sortLatestFirst(result.items),
        ];
        this.currentPage = result.page;
        this.totalFeedback = result.totalCount;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.isNextPageLoading = false;
        this.errorMessage = error.name === 'TimeoutError'
          ? 'Next feedback page request timed out. Please make sure the backend API is running at http://127.0.0.1:5048.'
          : 'Unable to load the next 10 feedback records from backend.';
        this.cdr.markForCheck();
      },
    });
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

  get reviewPieChartBackground(): string {
    if (this.totalFeedback === 0) return '#e5e7eb';

    const positiveEnd = this.toPiePercent(this.positiveCount);
    const negativeEnd = positiveEnd + this.toPiePercent(this.negativeCount);

    return `conic-gradient(#16a34a 0 ${positiveEnd}%, #dc2626 ${positiveEnd}% ${negativeEnd}%, #ca8a04 ${negativeEnd}% 100%)`;
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
    return this.feedbackList;
  }

  get hasMoreFeedback(): boolean {
    return this.feedbackList.length < this.totalFeedback;
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

  private loadOverview() {
    this.isListLoading = true;
    this.errorMessage = '';

    this.api.getFeedback().subscribe({
      next: (feedback) => {
        this.isListLoading = false;
        const latestFeedback = this.sortLatestFirst(feedback);
        this.feedbackList = latestFeedback.slice(0, this.pageSize);
        this.topNegativeFeedbacks = this.toNegativeInsights(this.getTopNegativeFeedbacks(latestFeedback));
        this.totalFeedback = latestFeedback.length;
        this.positiveCount = this.countBySentiment(latestFeedback, 'POSITIVE');
        this.negativeCount = this.countBySentiment(latestFeedback, 'NEGATIVE');
        this.neutralCount = this.countBySentiment(latestFeedback, 'NEUTRAL');
        this.currentPage = 1;
        this.hasLoadedOverview = true;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.isListLoading = false;
        this.hasLoadedOverview = true;
        this.errorMessage = error.name === 'TimeoutError'
          ? 'Feedback records request timed out. Please make sure the backend API is running at http://127.0.0.1:5048.'
          : 'Unable to load feedback records from backend. Please start the backend API and try again.';
        this.cdr.markForCheck();
      },
    });
  }

  private toNegativeInsights(feedback: FeedbackItem[]): NegativeFeedbackInsight[] {
    if (!Array.isArray(feedback)) return [];

    const repeatCounts: { [key: string]: number } = {};
    for (let item of feedback) {
      const issueCategory = this.getNegativeIssueCategory(item);
      if (repeatCounts[issueCategory]) {
        repeatCounts[issueCategory] += 1;
      } else {
        repeatCounts[issueCategory] = 1;
      }
    }

    const result: NegativeFeedbackInsight[] = [];
    for (let item of feedback) {
      const issueCategory = this.getNegativeIssueCategory(item);
      result.push({
        feedback: item,
        repeatCount: repeatCounts[issueCategory] || 1,
      });
    }

    return result;
  }

  private sortLatestFirst(feedback: FeedbackItem[]): FeedbackItem[] {
    if (!Array.isArray(feedback)) return [];

    return [...feedback].sort((a, b) => {
      return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    });
  }

  private countBySentiment(feedback: FeedbackItem[], sentiment: string): number {
    return feedback.filter((item) => item.sentiment === sentiment).length;
  }

  private getTopNegativeFeedbacks(feedback: FeedbackItem[]): FeedbackItem[] {
    const negativeFeedback: FeedbackItem[] = [];
    for (let item of feedback) {
      if (item.sentiment === 'NEGATIVE') {
        negativeFeedback.push(item);
      }
    }

    const repeatCounts: { [key: string]: number } = {};
    for (let item of negativeFeedback) {
      const issueCategory = this.getNegativeIssueCategory(item);
      if (repeatCounts[issueCategory]) {
        repeatCounts[issueCategory] += 1;
      } else {
        repeatCounts[issueCategory] = 1;
      }
    }

    negativeFeedback.sort((a, b) => {
      const catA = this.getNegativeIssueCategory(a);
      const catB = this.getNegativeIssueCategory(b);
      const countA = repeatCounts[catA] || 0;
      const countB = repeatCounts[catB] || 0;

      if (countB !== countA) {
        return countB - countA;
      }
      return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    });

    return negativeFeedback.slice(0, 5);
  }
}
