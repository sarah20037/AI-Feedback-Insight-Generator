import '@angular/compiler';
import { of } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';
import { CustomerFeedbackFormComponent } from './customer-feedback-form';

describe('CustomerFeedbackFormComponent', () => {
  it('requires feedback text', () => {
    const component = new CustomerFeedbackFormComponent({} as any, { detectChanges: vi.fn() } as any);
    component.customerId = 1;

    component.submitFeedback();

    expect(component.errorMessage).toBe('Please write feedback before submitting.');
  });

  it('requires logged in customer', () => {
    const component = new CustomerFeedbackFormComponent({} as any, { detectChanges: vi.fn() } as any);
    component.feedbackText = 'Good';

    component.submitFeedback();

    expect(component.errorMessage).toBe('Please login again before submitting feedback.');
  });

  it('saves feedback and resets form', () => {
    const api = { submitFeedback: vi.fn().mockReturnValue(of({})) };
    const component = new CustomerFeedbackFormComponent(api as any, { detectChanges: vi.fn() } as any);
    let saved = false;
    component.feedbackSaved.subscribe(() => saved = true);
    component.customerId = 3;
    component.feedbackText = 'Good experience';

    component.submitFeedback();

    expect(api.submitFeedback).toHaveBeenCalledWith(3, 'Good experience');
    expect(component.hasSubmittedFeedback).toBe(true);
    expect(component.feedbackText).toBe('');
    expect(saved).toBe(true);
  });
});
