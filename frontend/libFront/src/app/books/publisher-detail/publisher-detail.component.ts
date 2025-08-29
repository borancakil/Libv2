import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { Publisher } from '../models/publisher.model';
import { Book } from '../models/book.model';
import { PublisherApiService } from '../services/publisher-api';

@Component({
  selector: 'app-publisher-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, FormsModule],
  templateUrl: './publisher-detail.component.html',
  styleUrls: ['./publisher-detail.component.css'],
})
export class PublisherDetailComponent implements OnInit, OnDestroy {
  publisher: Publisher | null = null;
  books: Book[] = [];
  isLoading = true;
  isLoadingBooks = false;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  // Photo properties
  publisherPhoto: SafeUrl | null = null;
  isPhotoLoading = false;

  // Search and filter properties
  filteredBooks: Book[] = [];
  searchTerm = '';
  selectedFilter = 'all';
  selectedSort = 'title';
  searchTimeout: any;

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private publisherApi: PublisherApiService,
    public translate: TranslateService,
    private sanitizer: DomSanitizer,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser)
      return; // SSR sırasında API çağrısı yapma

    this.loadPublisher();
  }

  private loadPublisher(): void {
    this.isLoading = true;
    this.error = null;

    const publisherId = Number(this.route.snapshot.paramMap.get('id'));
    if (!publisherId) {
      this.handleError('INVALID_PUBLISHER_ID');
      return;
    }

    const subscription = this.publisherApi.getById(publisherId).subscribe({
      next: (publisher) => {
        this.publisher = publisher;
        this.isLoading = false;
        this.loadPhoto(publisherId);
        this.loadBooks(publisherId);
      },
      error: (err) => {
        console.error('Error loading publisher:', err);
        this.isLoading = false;
        this.handleError('PUBLISHER_DETAIL_LOAD_ERROR');
      },
    });

    this.subscriptions.push(subscription);
  }

  private loadPhoto(publisherId: number): void {
    this.isPhotoLoading = true;

    const subscription = this.publisherApi.getPublisherLogoImage(publisherId).subscribe({
      next: (blob: Blob) => {
        const url = URL.createObjectURL(blob);
        this.publisherPhoto = this.sanitizer.bypassSecurityTrustUrl(url);
        this.isPhotoLoading = false;
      },
      error: (err: any) => {
        console.error(`Error loading photo for publisher ${publisherId}:`, err);
        this.isPhotoLoading = false;
      }
    });

    this.subscriptions.push(subscription);
  }

  private loadBooks(publisherId: number): void {
    this.isLoadingBooks = true;

    const subscription = this.publisherApi.getBooksByPublisher(publisherId).subscribe({
      next: (books: Book[]) => {
        this.books = books;
        this.filteredBooks = [...books];
        this.isLoadingBooks = false;
      },
      error: (err: any) => {
        console.error('Error loading books:', err);
        this.isLoadingBooks = false;
        this.handleError('BOOKS_LOAD_ERROR');
      },
    });

    this.subscriptions.push(subscription);
  }

  onImageError(event: Event): void {
    console.error('Publisher image error:', event);
    this.publisherPhoto = null;
  }

  onBookImageError(event: Event): void {
    console.error('Book image error:', event);
    const target = event.target as HTMLImageElement;
    if (target) {
      target.style.display = 'none';
    }
  }

  getAvailableBooksCount(): number {
    return this.books.filter(book => book.isAvailable).length;
  }

  // Search and filter methods
  onSearchInput(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.performSearch();
    }, 300);
  }

  performSearch(): void {
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  onSortChange(): void {
    this.applyFilters();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedFilter = 'all';
    this.selectedSort = 'title';
    this.applyFilters();
  }

  hasActiveFilters(): boolean {
    return this.searchTerm.trim() !== '' || this.selectedFilter !== 'all';
  }

  private applyFilters(): void {
    let filtered = [...this.books];

    // Apply search filter
    if (this.searchTerm.trim() !== '') {
      const searchLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(book => 
        book.title.toLowerCase().includes(searchLower) ||
        (book.author && book.author.name.toLowerCase().includes(searchLower)) ||
        (book.publicationYear && book.publicationYear.toString().includes(searchLower))
      );
    }

    // Apply availability filter
    switch (this.selectedFilter) {
      case 'available':
        filtered = filtered.filter(book => book.isAvailable);
        break;
      case 'unavailable':
        filtered = filtered.filter(book => !book.isAvailable);
        break;
    }

    // Apply sorting
    switch (this.selectedSort) {
      case 'title':
        filtered.sort((a, b) => a.title.localeCompare(b.title));
        break;
      case 'year':
        filtered.sort((a, b) => {
          const aYear = a.publicationYear || 0;
          const bYear = b.publicationYear || 0;
          return bYear - aYear;
        });
        break;
      case 'author':
        filtered.sort((a, b) => {
          const aAuthor = a.author?.name || '';
          const bAuthor = b.author?.name || '';
          return aAuthor.localeCompare(bAuthor);
        });
        break;
    }

    this.filteredBooks = filtered;
  }

  goToBookDetail(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'book', bookId]);
  }

  goBack(): void {
    this.router.navigate(['/publishers']);
  }

  retry(): void {
    this.loadPublisher();
  }

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message) => {
      this.error = message;
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    
    // Clean up blob URLs
    if (this.publisherPhoto && typeof this.publisherPhoto === 'string') {
      URL.revokeObjectURL(this.publisherPhoto);
    }
  }
} 