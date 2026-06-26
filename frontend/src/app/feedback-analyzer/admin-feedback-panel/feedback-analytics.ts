import { FeedbackItem } from '../../services/api.models';

export interface NegativeFeedbackInsight {
  feedback: FeedbackItem;
  repeatCount: number;
}

export interface FeedbackStats {
  topNegativeFeedbacks: NegativeFeedbackInsight[];
  totalFeedback: number;
  positiveCount: number;
  negativeCount: number;
  neutralCount: number;
  positivePercent: number;
  negativePercent: number;
  neutralPercent: number;
  mostFrequentNegativeIssue: string;
  topRecommendedAction: string;
  reviewPieChartBackground: string;
}

export function getNegativeIssueCategory(feedback: FeedbackItem): string {
  return feedback.issueCategory || 'General negative feedback';
}

export function toPiePercent(count: number, total: number): number {
  return total === 0 ? 0 : (count / total) * 100;
}

export function toRoundedPercent(count: number, total: number): number {
  return Math.round(toPiePercent(count, total));
}

export function sortLatestFirst(feedback: FeedbackItem[]): FeedbackItem[] {
  return Array.isArray(feedback) ? [...feedback].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()) : [];
}

export function countBySentiment(feedback: FeedbackItem[], sentiment: string): number {
  return feedback.filter((item) => item.sentiment === sentiment).length;
}

export function toNegativeInsights(feedback: FeedbackItem[]): NegativeFeedbackInsight[] {
  if (!Array.isArray(feedback)) return [];
  const repeatCounts: Record<string, number> = {};
  for (const item of feedback) {
    const cat = getNegativeIssueCategory(item);
    repeatCounts[cat] = (repeatCounts[cat] || 0) + 1;
  }
  return feedback.map(item => ({
    feedback: item,
    repeatCount: repeatCounts[getNegativeIssueCategory(item)] || 1,
  }));
}

export function getTopNegativeFeedbacks(feedback: FeedbackItem[]): FeedbackItem[] {
  const negativeFeedback = feedback.filter(item => item.sentiment === 'NEGATIVE');
  const repeatCounts: Record<string, number> = {};
  for (const item of negativeFeedback) {
    const cat = getNegativeIssueCategory(item);
    repeatCounts[cat] = (repeatCounts[cat] || 0) + 1;
  }
  return negativeFeedback.sort((a, b) => {
    const countDiff = (repeatCounts[getNegativeIssueCategory(b)] || 0) - (repeatCounts[getNegativeIssueCategory(a)] || 0);
    return countDiff !== 0 ? countDiff : new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  }).slice(0, 5);
}

export function getMostFrequentNegativeIssue(topNegativeFeedbacks: NegativeFeedbackInsight[]): string {
  return topNegativeFeedbacks[0]?.feedback.feedbackText || 'No repeated negative feedback yet';
}

export function getTopRecommendedAction(topNegativeFeedbacks: NegativeFeedbackInsight[], negativeCount: number): string {
  if (topNegativeFeedbacks[0]) return topNegativeFeedbacks[0].feedback.recommendedAction;
  return negativeCount > 0
    ? 'Read recent negative feedback, group similar complaints, and assign one owner for follow-up.'
    : 'Keep monitoring feedback and continue reinforcing what customers already value.';
}

export function getPieChartBackground(totalFeedback: number, positiveCount: number, negativeCount: number): string {
  if (totalFeedback === 0) return '#e5e7eb';
  const pos = toPiePercent(positiveCount, totalFeedback);
  const neg = pos + toPiePercent(negativeCount, totalFeedback);
  return `conic-gradient(#16a34a 0 ${pos}%, #dc2626 ${pos}% ${neg}%, #f2f20b ${neg}% 100%)`;
}

export function getFeedbackStats(feedback: FeedbackItem[]): FeedbackStats {
  const topNegativeFeedbacks = toNegativeInsights(getTopNegativeFeedbacks(feedback));
  const totalFeedback = feedback.length;
  const positiveCount = countBySentiment(feedback, 'POSITIVE');
  const negativeCount = countBySentiment(feedback, 'NEGATIVE');
  const neutralCount = countBySentiment(feedback, 'NEUTRAL');

  return {
    topNegativeFeedbacks,
    totalFeedback,
    positiveCount,
    negativeCount,
    neutralCount,
    positivePercent: toRoundedPercent(positiveCount, totalFeedback),
    negativePercent: toRoundedPercent(negativeCount, totalFeedback),
    neutralPercent: toRoundedPercent(neutralCount, totalFeedback),
    mostFrequentNegativeIssue: getMostFrequentNegativeIssue(topNegativeFeedbacks),
    topRecommendedAction: getTopRecommendedAction(topNegativeFeedbacks, negativeCount),
    reviewPieChartBackground: getPieChartBackground(totalFeedback, positiveCount, negativeCount),
  };
}
