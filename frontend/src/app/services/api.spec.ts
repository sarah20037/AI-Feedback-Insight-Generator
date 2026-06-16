import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApiService } from './api';
import { environment } from '../../environments/environment';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('submits feedback with the selected customer id and text', () => {
    service.submitFeedback(7, 'The HR portal is fast.').subscribe();

    const request = httpMock.expectOne(`${environment.apiUrl}/feedback/submit`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      customerId: 7,
      feedbackText: 'The HR portal is fast.',
    });
    request.flush({});
  });

  it('requests paged feedback with page and page size params', () => {
    service.getFeedbackPage(2, 10).subscribe();

    const request = httpMock.expectOne((req) => req.url === `${environment.apiUrl}/feedback/page`);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('10');
    request.flush({ items: [], page: 2, pageSize: 10, totalCount: 0, positiveCount: 0, negativeCount: 0, neutralCount: 0 });
  });
});
