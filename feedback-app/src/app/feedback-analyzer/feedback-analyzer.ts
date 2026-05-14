import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

type FeedbackSentiment = 'Good' | 'Bad' | 'Neutral';

interface FeedbackItem {
  id: number;
  text: string;
  sentiment: FeedbackSentiment;
  issue: string;
  action: string;
  submittedBy: string;
  createdAt: string;
}

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
  successMessage: string = '';
  feedbackList: FeedbackItem[] = [
    {
      id: 1,
      text: 'The HR support response is slow and I had to wait too long for updates.',
      sentiment: 'Bad',
      issue: 'Slow response or delays',
      action: 'Review response timelines, assign clear owners, and share realistic turnaround times with users.',
      submittedBy: 'Asha',
      createdAt: new Date().toISOString(),
    },
    {
      id: 2,
      text: 'The new process is clear and helpful.',
      sentiment: 'Good',
      issue: 'Positive feedback',
      action: 'Share this feedback with the team and continue the current approach.',
      submittedBy: 'Ravi',
      createdAt: new Date().toISOString(),
    },
    {
      id: 3,
      text: 'The portal form is confusing.',
      sentiment: 'Bad',
      issue: 'Difficult or confusing process',
      action: 'Simplify the workflow, improve instructions, and test the process with a small user group.',
      submittedBy: 'Meera',
      createdAt: new Date().toISOString(),
    },
  ];

  submitFeedback() {
    const text = this.feedbackText.trim();

    if (!text) return;

    const feedback: FeedbackItem = {
      id: Date.now(),
      text,
      sentiment: 'Neutral',
      issue: 'General feedback',
      action: 'Review this feedback manually and decide the next owner, fix, or follow-up message.',
      submittedBy: this.userName,
      createdAt: new Date().toISOString(),
    };

    this.feedbackList = [feedback, ...this.feedbackList];
    this.successMessage = 'Feedback received. Thank you for sharing your thoughts.';
    this.feedbackText = '';
  }

  logout() {
    if (typeof window === 'undefined') return;

    localStorage.removeItem('role');
    localStorage.removeItem('user');
    location.reload();
  }

  get totalFeedback(): number {
    return this.feedbackList.length;
  }

  get positiveCount(): number {
    return this.countBySentiment('Good');
  }

  get negativeCount(): number {
    return this.countBySentiment('Bad');
  }

  get neutralCount(): number {
    return this.countBySentiment('Neutral');
  }

  get mostFrequentNegativeIssue(): string {
    const negativeFeedback = this.feedbackList.filter((feedback) => feedback.sentiment === 'Bad');

    if (negativeFeedback.length === 0) return 'No repeated negative issue yet';

    const issueCounts = negativeFeedback.reduce<Record<string, number>>((counts, feedback) => {
      const issue = feedback.issue || 'General negative feedback';
      counts[issue] = (counts[issue] || 0) + 1;
      return counts;
    }, {});

    return Object.entries(issueCounts).sort((a, b) => b[1] - a[1])[0][0];
  }

  get topRecommendedAction(): string {
    const mostFrequentIssue = this.mostFrequentNegativeIssue;
    const matchedFeedback = this.feedbackList.find((feedback) => feedback.issue === mostFrequentIssue);

    if (matchedFeedback) return matchedFeedback.action;
    if (this.negativeCount > 0) return 'Read recent negative feedback, group similar complaints, and assign one owner for follow-up.';

    return 'Keep monitoring feedback and continue reinforcing what customers already value.';
  }

  get negativeFeedbackList(): FeedbackItem[] {
    return this.feedbackList.filter((feedback) => feedback.sentiment === 'Bad');
  }

  private countBySentiment(sentiment: FeedbackSentiment): number {
    return this.feedbackList.filter((feedback) => feedback.sentiment === sentiment).length;
  }

  private getStoredValue(key: string): string | null {
    if (typeof window === 'undefined') return null;

    return localStorage.getItem(key);
  }
}
