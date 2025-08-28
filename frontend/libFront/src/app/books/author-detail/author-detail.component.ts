import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Author } from '../models/author.model';
import { Book } from '../models/book.model';
import { AuthorApiService } from '../services/author-api';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-author-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, FormsModule],
  templateUrl: './author-detail.component.html',
  styleUrls: ['./author-detail.component.css'],
})
export class AuthorDetailComponent implements OnInit, OnDestroy {
  author: Author | null = null;
  books: Book[] = [];
  filteredBooks: Book[] = [];
  isLoading = true;
  isLoadingBooks = false;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  // Photo properties
  authorPhoto: SafeUrl | null = null;
  isPhotoLoading = false;

  // Search and filter properties
  searchTerm = '';
  selectedFilter = 'all';
  selectedSort = 'title';
  private searchTimeout: any;

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    private authorApi: AuthorApiService,
    public translate: TranslateService,
    private sanitizer: DomSanitizer,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser) {
      return;
    }

    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (idParam) {
      const authorId = parseInt(idParam, 10);
      
      if (!isNaN(authorId)) {
        this.loadAuthor(authorId);
        this.loadBooksByAuthor(authorId);
      } else {
        this.handleError('AUTHOR_DETAIL_INVALID_ID');
      }
    } else {
      this.handleError('AUTHOR_DETAIL_NO_ID');
    }
  }

  private loadAuthor(id: number): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.authorApi.getById(id).subscribe({
      next: (author) => {
        this.author = author;
        this.isLoading = false;
        this.loadAuthorPhoto(id);
      },
      error: (err) => {
        console.error('Error loading author:', err);
        this.isLoading = false;
        
        if (err.status === 404) {
          this.handleError('AUTHOR_DETAIL_NOT_FOUND');
        } else {
          this.handleError('AUTHOR_DETAIL_LOAD_ERROR');
        }
      },
    });

    this.subscriptions.push(subscription);
  }

  private loadAuthorPhoto(authorId: number): void {
    if (!this.author?.hasProfileImage) {
      return;
    }

    this.isPhotoLoading = true;

    const subscription = this.authorApi.getProfileImage(authorId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        this.authorPhoto = this.sanitizer.bypassSecurityTrustUrl(url);
        this.isPhotoLoading = false;
      },
      error: (err) => {
        console.error(`Error loading photo for author ${authorId}:`, err);
        this.isPhotoLoading = false;
      }
    });

    this.subscriptions.push(subscription);
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
    const placeholder = img.parentElement?.querySelector('.photo-placeholder') as HTMLElement;
    if (placeholder) {
      placeholder.style.display = 'flex';
    }
  }

  // Search and filter methods
  onSearchInput(): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    
    this.searchTimeout = setTimeout(() => {
      this.applyFilters();
    }, 300); // Debounce search
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
        (book.publisherName && book.publisherName.toLowerCase().includes(searchLower)) ||
        book.bookId.toString().includes(searchLower)
      );
    }

    // Apply availability filter
    if (this.selectedFilter !== 'all') {
      filtered = filtered.filter(book => {
        if (this.selectedFilter === 'available') {
          return book.isAvailable;
        } else if (this.selectedFilter === 'unavailable') {
          return !book.isAvailable;
        }
        return true;
      });
    }

    // Apply sorting
    filtered.sort((a, b) => {
      switch (this.selectedSort) {
        case 'title':
          return a.title.localeCompare(b.title);
        case 'year':
          return (b.publicationYear || 0) - (a.publicationYear || 0);
        case 'publisher':
          return (a.publisherName || '').localeCompare(b.publisherName || '');
        default:
          return 0;
      }
    });

    this.filteredBooks = filtered;
  }

  getAvailableBooksCount(): number {
    return this.filteredBooks.filter(book => book.isAvailable).length;
  }

  goToPublisherDetail(publisherId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'publisher', publisherId]);
  }

  private loadBooksByAuthor(authorId: number): void {
    this.isLoadingBooks = true;

    const subscription = this.authorApi.getBooksByAuthor(authorId).subscribe({
      next: (books) => {
        this.books = books;
        this.filteredBooks = books;
        this.isLoadingBooks = false;
      },
      error: (err) => {
        console.error('Error loading books by author:', err);
        this.isLoadingBooks = false;
        // Kitaplar yüklenemese bile yazar bilgileri gösterilebilir
      },
    });

    this.subscriptions.push(subscription);
  }

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message: string) => {
      this.error = message;
    });
    this.isLoading = false;
  }

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'authors']);
  }

  goToBookDetail(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'book', bookId]);
  }

  retry(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const authorId = parseInt(idParam, 10);
      if (!isNaN(authorId)) {
        this.loadAuthor(authorId);
        this.loadBooksByAuthor(authorId);
      }
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    // Clean up blob URL
    if (this.authorPhoto && typeof this.authorPhoto === 'string') {
      URL.revokeObjectURL(this.authorPhoto);
    }
  }
} 