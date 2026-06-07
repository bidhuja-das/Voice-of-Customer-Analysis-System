import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FeedbackService, Feedback } from '../../core/services/feedback.service';
import { SentimentCountPipe, PendingCountPipe } from '../../core/pipes/sentiment-count.pipe';
import { ToastService } from '../../core/services/toast.service';

// Add to imports array:


@Component({
  selector: 'app-feedback-list',
  standalone: true,
  imports: [CommonModule, FormsModule, SentimentCountPipe, PendingCountPipe],
  templateUrl: './feedback-list.component.html',
  styleUrls: ['./feedback-list.component.scss']
})
export class FeedbackListComponent implements OnInit {
  feedbacks: Feedback[] = [];
  filteredFeedbacks: Feedback[] = [];
  loading = false;
  uploadLoading = false;
  uploadMessage = '';
  uploadError = '';
  searchText = '';
  selectedSentiment = '';
  selectedSource = '';

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;

  constructor(
    private feedbackService: FeedbackService,
  private toast: ToastService) {}

  ngOnInit() {
    this.loadFeedbacks();
  }

  loadFeedbacks() {
    this.loading = true;
    this.feedbackService.getAll().subscribe({
      next: (data) => {
        this.feedbacks = data;
        this.applyFilters();
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  applyFilters() {
    let result = [...this.feedbacks];

    if (this.searchText) {
      const search = this.searchText.toLowerCase();
      result = result.filter(f =>
        f.rawText.toLowerCase().includes(search) ||
        f.customerIdentifier.toLowerCase().includes(search)
      );
    }

    if (this.selectedSentiment) {
      result = result.filter(f => f.sentiment === this.selectedSentiment);
    }

    if (this.selectedSource) {
      result = result.filter(f => f.sourceName === this.selectedSource);
    }

    this.filteredFeedbacks = result;
    this.totalPages = Math.ceil(result.length / this.pageSize);
    this.currentPage = 1;
  }

  get paginatedFeedbacks(): Feedback[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredFeedbacks.slice(start, start + this.pageSize);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get uniqueSources(): string[] {
    return [...new Set(this.feedbacks.map(f => f.sourceName))];
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  onFileUpload(event: any) {
  const file = event.target.files[0];
  if (!file) return;

  this.uploadLoading = true;
  this.uploadMessage = '';
  this.uploadError = '';

  this.feedbackService.bulkIngest(file).subscribe({
    next: (res) => {
      this.uploadLoading = false;
      this.uploadMessage = res.message;
      if (res.errors?.length > 0) {
        this.uploadError = `${res.errors.length} rows had errors and were skipped.`;
      }
      this.loadFeedbacks();
    },
    error: () => {
      this.uploadLoading = false;
      this.uploadError = 'Upload failed. Please check your CSV format.';
    }
  });
}

  getSentimentClass(sentiment: string | null): string {
    if (!sentiment) return '';
    return 'badge-' + sentiment.toLowerCase();
  }

  clearFilters() {
    this.searchText = '';
    this.selectedSentiment = '';
    this.selectedSource = '';
    this.applyFilters();
  }

  // Add these new properties
showAddModal = false;
newFeedback = {
  sourceId: 1,
  customerIdentifier: '',
  rawText: '',
  submittedAt: new Date().toISOString().split('T')[0]
};
addLoading = false;
addMessage = '';
addError = '';

// Add these new methods
openAddModal() {
  this.showAddModal = true;
  this.addMessage = '';
  this.addError = '';
  this.newFeedback = {
    sourceId: 1,
    customerIdentifier: '',
    rawText: '',
    submittedAt: new Date().toISOString().split('T')[0]
  };
}

closeAddModal() {
  this.showAddModal = false;
}

submitFeedback() {
  this.addError = '';

  if (!this.newFeedback.rawText.trim()) {
    this.addError = 'Feedback text is required.';
    return;
  }

  this.addLoading = true;

  this.feedbackService.ingestOne({
    sourceId: this.newFeedback.sourceId,
    customerIdentifier: this.newFeedback.customerIdentifier || 'anonymous',
    rawText: this.newFeedback.rawText,
    submittedAt: this.newFeedback.submittedAt
  }).subscribe({
    next: () => {
      this.addLoading = false;
      this.addMessage = 'Feedback added successfully!';
      this.loadFeedbacks();
      setTimeout(() => this.closeAddModal(), 1500);
    },
    error: (err) => {
      this.addLoading = false;
      this.addError = err.error?.error || 'Failed to add feedback. Please try again.';
    }
  });
}
  
}