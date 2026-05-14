import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from './login/login';
import { FeedbackAnalyzerComponent } from './feedback-analyzer/feedback-analyzer';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, LoginComponent, FeedbackAnalyzerComponent],
  templateUrl: './app.html',
})
export class AppComponent implements OnInit {

  role: string | null = null;

  ngOnInit() {
    if (typeof window !== 'undefined') {
      this.role = localStorage.getItem('role');
    }
  }
}