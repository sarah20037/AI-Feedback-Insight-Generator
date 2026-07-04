import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { timeout } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export type FeedbackSentiment = 'POSITIVE' | 'NEGATIVE' | 'NEUTRAL';

export interface LoginResponse {
  message: string;
  customerId?: number;
  username: string;
  fullName?: string;
  role?: 'customer' | 'admin';
}

export interface AnalysisResult {
  summary: string;
  sentiment: FeedbackSentiment;
  category: string;
  recommendedAction: string;
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
  aiAnalysis: AnalysisResult;
}

export interface FeedbackOverview {
  items: FeedbackItem[];
  latestNegativeItems: FeedbackItem[];
  totalCount: number;
  positiveCount: number;
  negativeCount: number;
  neutralCount: number;
}

export interface FeedbackPageResult {
  items: FeedbackItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  positiveCount: number;
  negativeCount: number;
  neutralCount: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  register(name: string, email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/register`, {
      fullName: name,
      email,
      username: email,
      passwordHash: password,
    }).pipe(timeout(8000));
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, {
      username: email,
      passwordHash: password,
    }).pipe(timeout(8000));
  }

  adminLogin(username: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/admin-login`, {
      username,
      passwordHash: password,
    }).pipe(timeout(8000));
  }

  submitFeedback(customerId: number, feedbackText: string): Observable<SubmitFeedbackResponse> {
    return this.http.post<SubmitFeedbackResponse>(`${this.baseUrl}/feedback/submit`, {
      customerId,
      feedbackText,
    }).pipe(timeout(10000));
  }

  getFeedback(): Observable<FeedbackItem[]> {
    return this.http.get<FeedbackItem[]>(`${this.baseUrl}/feedback`).pipe(timeout(8000));
  }

  getFeedbackOverview(): Observable<FeedbackOverview> {
    return this.http.get<FeedbackOverview>(`${this.baseUrl}/feedback/overview`).pipe(timeout(8000));
  }

  getFeedbackPage(page: number, pageSize = 10): Observable<FeedbackPageResult> {
    return this.http.get<FeedbackPageResult>(`${this.baseUrl}/feedback/page`, {
      params: {
        page,
        pageSize,
      },
    }).pipe(timeout(8000));
  }

  getCustomerFeedback(customerId: number): Observable<FeedbackItem[]> {
    return this.http.get<FeedbackItem[]>(`${this.baseUrl}/feedback/customer/${customerId}`).pipe(timeout(8000));
  }
}
