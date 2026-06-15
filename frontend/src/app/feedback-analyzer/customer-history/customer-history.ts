import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { ApiService, FeedbackItem } from '../../services/api';

@Component({
  selector: 'app-customer-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-history.html',
  styleUrls: ['../feedback-analyzer.css'],
})
export class CustomerHistoryComponent implements OnChanges {
  @Input() customerId: number | null = null;
  @Input() refreshKey = 0;
  @Output() historyCountChanged = new EventEmitter<number>();

  pastFeedbacks: FeedbackItem[] = [];
  isLoading = false;
  errorMessage = '';
  hasRequestedHistory = false;

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnChanges(changes: SimpleChanges) {
    if (this.hasRequestedHistory && changes['refreshKey'] && !changes['refreshKey'].firstChange) {
      this.loadPastFeedbacks();
    }
  }

  loadPastFeedbacks() {
    if (!this.customerId) return;

    this.hasRequestedHistory = true;
    this.isLoading = true;
    this.errorMessage = '';

    this.api.getCustomerFeedback(this.customerId).pipe(
      finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (data) => {
        this.pastFeedbacks = this.sortLatestFirst(data);
        this.historyCountChanged.emit(this.pastFeedbacks.length);
      },
      error: () => {
        this.errorMessage = 'Could not load your past feedback records.';
      },
    });
  }

  private sortLatestFirst(data: FeedbackItem[]): FeedbackItem[] {
    if (!Array.isArray(data)) return [];

    return [...data].sort((a, b) => {
      return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    });
  }
}
