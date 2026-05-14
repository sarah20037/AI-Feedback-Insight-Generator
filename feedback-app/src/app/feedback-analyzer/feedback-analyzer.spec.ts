import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeedbackAnalyzerComponent } from './feedback-analyzer';

describe('FeedbackAnalyzerComponent', () => {
  let component: FeedbackAnalyzerComponent;
  let fixture: ComponentFixture<FeedbackAnalyzerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeedbackAnalyzerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FeedbackAnalyzerComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});