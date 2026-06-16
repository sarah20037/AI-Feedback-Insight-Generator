import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ApiService, FeedbackItem, FeedbackPageResult } from '../../services/api';
import { AdminFeedbackPanelComponent } from './admin-feedback-panel';

describe('AdminFeedbackPanelComponent', () => {
  let fixture: ComponentFixture<AdminFeedbackPanelComponent>;
  let component: AdminFeedbackPanelComponent;
  let api: {
    getFeedback: ReturnType<typeof vi.fn>;
    getFeedbackPage: ReturnType<typeof vi.fn>;
  };

  const feedback = (feedbackId: number, sentiment: FeedbackItem['sentiment'], createdAt: string): FeedbackItem => ({
    feedbackId,
    customerId: feedbackId,
    customerName: `Customer ${feedbackId}`,
    customerEmail: `customer${feedbackId}@example.com`,
    feedbackText: `Feedback ${feedbackId}`,
    summary: `Summary ${feedbackId}`,
    sentiment,
    issueCategory: sentiment === 'NEGATIVE' ? 'Support' : 'General',
    recommendedAction: `Action ${feedbackId}`,
    submittedBy: `Customer ${feedbackId}`,
    createdAt,
  });

  const createComponent = (items: FeedbackItem[]) => {
    api.getFeedback.mockReturnValue(of(items));
    fixture = TestBed.createComponent(AdminFeedbackPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  beforeEach(async () => {
    api = {
      getFeedback: vi.fn(),
      getFeedbackPage: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [AdminFeedbackPanelComponent],
      providers: [{ provide: ApiService, useValue: api as Partial<ApiService> }],
    }).compileComponents();
  });

  it('loads and summarizes the first feedback page', () => {
    createComponent([
      feedback(1, 'POSITIVE', '2026-06-14T08:00:00Z'),
      feedback(2, 'NEGATIVE', '2026-06-15T08:00:00Z'),
      feedback(3, 'NEUTRAL', '2026-06-13T08:00:00Z'),
    ]);

    expect(component.feedbackList.map((item) => item.feedbackId)).toEqual([2, 1, 3]);
    expect(component.totalFeedback).toBe(3);
    expect(component.positiveCount).toBe(1);
    expect(component.negativeCount).toBe(1);
    expect(component.neutralCount).toBe(1);
  });

  it('renders the load more button after the displayed feedback cards', () => {
    createComponent(Array.from({ length: 11 }, (_, index) => (
      feedback(index + 1, 'POSITIVE', `2026-06-${String(index + 1).padStart(2, '0')}T08:00:00Z`)
    )));

    const host = fixture.nativeElement as HTMLElement;
    const cards = Array.from(host.querySelectorAll('.feedback-card'));
    const loadMoreButton = host.querySelector('button.ghost-button:last-of-type');

    expect(cards.length).toBe(10);
    expect(loadMoreButton?.textContent?.trim()).toBe('Load Next 10');
    expect(cards.at(-1)?.compareDocumentPosition(loadMoreButton as Node)).toBe(Node.DOCUMENT_POSITION_FOLLOWING);
  });

  it('appends the next feedback page', () => {
    createComponent([
      feedback(1, 'POSITIVE', '2026-06-14T08:00:00Z'),
      feedback(2, 'NEGATIVE', '2026-06-15T08:00:00Z'),
      feedback(3, 'NEUTRAL', '2026-06-13T08:00:00Z'),
    ]);

    const nextPage: FeedbackPageResult = {
      items: [feedback(4, 'POSITIVE', '2026-06-16T08:00:00Z')],
      page: 2,
      pageSize: 10,
      totalCount: 4,
      positiveCount: 2,
      negativeCount: 1,
      neutralCount: 1,
    };
    component.totalFeedback = 4;
    api.getFeedbackPage.mockReturnValue(of(nextPage));

    component.loadNextFeedbackPage();

    expect(api.getFeedbackPage).toHaveBeenCalledWith(2, 10);
    expect(component.feedbackList.map((item) => item.feedbackId)).toEqual([2, 1, 3, 4]);
    expect(component.currentPage).toBe(2);
  });
});
