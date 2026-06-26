import { describe, expect, it } from 'vitest';
import { FeedbackItem } from '../../services/api.models';
import { getFeedbackStats, sortLatestFirst } from './feedback-analytics';

function feedback(id: number, sentiment: FeedbackItem['sentiment'], category: string, date: string): FeedbackItem {
  return {
    feedbackId: id,
    customerId: id,
    customerName: `Customer ${id}`,
    customerEmail: `customer${id}@mail.com`,
    feedbackText: `Feedback ${id}`,
    summary: `Summary ${id}`,
    sentiment,
    issueCategory: category,
    recommendedAction: `Action ${id}`,
    submittedBy: `Customer ${id}`,
    createdAt: date,
  };
}

describe('feedback analytics', () => {
  it('sorts latest feedback first', () => {
    const list = [
      feedback(1, 'POSITIVE', 'General', '2026-06-20T10:00:00Z'),
      feedback(2, 'NEGATIVE', 'Support', '2026-06-22T10:00:00Z'),
      feedback(3, 'NEUTRAL', 'General', '2026-06-21T10:00:00Z'),
    ];

    expect(sortLatestFirst(list).map(item => item.feedbackId)).toEqual([2, 3, 1]);
  });

  it('counts sentiment and picks the main negative action', () => {
    const list = [
      feedback(1, 'NEGATIVE', 'Support', '2026-06-20T10:00:00Z'),
      feedback(2, 'NEGATIVE', 'Support', '2026-06-22T10:00:00Z'),
      feedback(3, 'POSITIVE', 'General', '2026-06-21T10:00:00Z'),
      feedback(4, 'NEUTRAL', 'General', '2026-06-19T10:00:00Z'),
    ];

    const stats = getFeedbackStats(list);

    expect(stats.totalFeedback).toBe(4);
    expect(stats.positiveCount).toBe(1);
    expect(stats.negativeCount).toBe(2);
    expect(stats.neutralCount).toBe(1);
    expect(stats.negativePercent).toBe(50);
    expect(stats.topRecommendedAction).toBe('Action 2');
  });
});
