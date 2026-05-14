import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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
    });
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, {
      username: email,
      passwordHash: password,
    });
  }

  adminLogin(username: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/admin-login`, {
      username,
      passwordHash: password,
    });
  }

  submitFeedback(customerName: string, customerEmail: string, feedbackText: string): Observable<SubmitFeedbackResponse> {
    return this.http.post<SubmitFeedbackResponse>(`${this.baseUrl}/feedback/submit`, {
      customerName,
      customerEmail,
      feedbackText,
    });
  }

  getFeedback(): Observable<FeedbackItem[]> {
    return this.http.get<FeedbackItem[]>(`${this.baseUrl}/feedback`);
  }
}
