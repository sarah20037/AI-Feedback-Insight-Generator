export type FeedbackSentiment = 'POSITIVE' | 'NEGATIVE' | 'NEUTRAL';
export type UserRole = 'customer' | 'admin';

export interface LoginSession {
  role: UserRole;
  username: string;
  customerId?: number;
  isFirstTimeUser?: boolean;
}

export interface LoginResponse {
  message: string;
  customerId?: number;
  username: string;
  fullName?: string;
  role?: UserRole;
}

export interface FeedbackItem {
  feedbackId: number;
  customerId: number;
  customerName: string;
  customerEmail: string;
  feedbackText: string;
  summary: string;
  sentiment: FeedbackSentiment;
  issueCategory: string;
  recommendedAction: string;
  submittedBy: string;
  createdAt: string;
}

export interface SubmitFeedbackResponse {
  message: string;
  feedback: FeedbackItem;
  aiAnalysis: {
    summary: string;
    sentiment: FeedbackSentiment;
    category: string;
    recommendedAction: string;
  };
}

export interface FeedbackOverview {
  items: FeedbackItem[];
  latestNegativeItems: FeedbackItem[];
  totalCount: number;
  positiveCount: number;
  negativeCount: number;
  neutralCount: number;
}

export interface FeedbackPageResult extends FeedbackOverview {
  page: number;
  pageSize: number;
}
