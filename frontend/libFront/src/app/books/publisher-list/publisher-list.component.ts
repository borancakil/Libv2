import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Subscription, forkJoin } from 'rxjs';
import { Publisher } from '../models/publisher.model';
import { PublisherApiService } from '../services/publisher-api';

@Component({
  selector: 'app-publisher-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, FormsModule],
  templateUrl: './publisher-list.component.html',
  styleUrls: ['./publisher-list.component.css'],
})
export class PublisherListComponent implements OnInit, OnDestroy {
  filter: string = '';
  publishers: Publisher[] = [];
  filteredPublishers: Publisher[] = [];
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  // Search and filter properties
  searchTerm = '';
  selectedFilter = 'all';
  selectedSort = 'name';
  searchTimeout: any;

  // Filter properties
  filters: any = {
    name: '',
    description: '',
    status: '',
    minBooks: undefined,
    maxBooks: undefined,
    foundedYear: undefined,
    location: '',
    sortBy: 'name'
  };
  
  // Filter expansion state
  isFilterExpanded = false;

  // Photo properties
  publisherPhotos: { [key: number]: SafeUrl } = {};
  loadingPhotos: { [key: number]: boolean } = {};

  // Book count properties
  publisherBookCounts: { [key: number]: number } = {};
  totalBooks = 0;

  constructor(
    public router: Router,
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

    this.loadPublishers();
  }

  private loadPublishers(): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.publisherApi.getAll(this.filter).subscribe({
      next: (publishers) => {
        this.publishers = publishers;
        this.filteredPublishers = [...publishers];
        this.isLoading = false;
        this.loadPublisherPhotos();
        this.loadBookCounts();
      },
      error: (err) => {
        console.error('Error loading publishers:', err);
        this.isLoading = false;
        this.handleError('PUBLISHER_LIST_LOAD_ERROR');
      },
    });

    this.subscriptions.push(subscription);
  }

  onFilterButtonClick(): void {
    this.filter = (this.filter || '').trim();
    this.loadPublishers();
  }

  private loadPublisherPhotos(): void {
    this.publishers.forEach(publisher => {
      this.loadPublisherPhoto(publisher.publisherId);
    });
  }

  private loadPublisherPhoto(publisherId: number): void {
    this.loadingPhotos[publisherId] = true;
    
    const subscription = this.publisherApi.getProfileImage(publisherId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        this.publisherPhotos[publisherId] = this.sanitizer.bypassSecurityTrustUrl(url);
        this.loadingPhotos[publisherId] = false;
      },
      error: (err) => {
        console.error(`Error loading photo for publisher ${publisherId}:`, err);
        this.loadingPhotos[publisherId] = false;
        // Don't show error for missing images, just skip them
      }
    });

    this.subscriptions.push(subscription);
  }

  private loadBookCounts(): void {
    if (this.publishers.length === 0) return;

    const bookCountRequests = this.publishers.map(publisher => 
      this.publisherApi.getBookCount(publisher.publisherId)
    );

    const subscription = forkJoin(bookCountRequests).subscribe({
      next: (counts) => {
        this.publishers.forEach((publisher, index) => {
          this.publisherBookCounts[publisher.publisherId] = counts[index];
        });
        this.calculateTotalBooks();
        this.applyFilters(); // Refresh filters with book counts
      },
      error: (err) => {
        console.error('Error loading book counts:', err);
        // If book count endpoint doesn't exist, try to get books for each publisher
        this.loadBooksForPublishers();
      }
    });

    this.subscriptions.push(subscription);
  }

  private loadBooksForPublishers(): void {
    const bookRequests = this.publishers.map(publisher => 
      this.publisherApi.getBooksByPublisher(publisher.publisherId)
    );

    const subscription = forkJoin(bookRequests).subscribe({
      next: (booksArrays) => {
        this.publishers.forEach((publisher, index) => {
          const books = booksArrays[index];
          this.publisherBookCounts[publisher.publisherId] = books.length;
          publisher.books = books; // Add books to publisher object
        });
        this.calculateTotalBooks();
        this.applyFilters(); // Refresh filters with book counts
      },
      error: (err) => {
        console.error('Error loading books for publishers:', err);
        // Set all book counts to 0 if both methods fail
        this.publishers.forEach(publisher => {
          this.publisherBookCounts[publisher.publisherId] = 0;
        });
        this.calculateTotalBooks();
      }
    });

    this.subscriptions.push(subscription);
  }

  private calculateTotalBooks(): void {
    this.totalBooks = Object.values(this.publisherBookCounts).reduce((total, count) => total + count, 0);
  }

  onImageError(event: Event, publisherId: number): void {
    console.error(`Image error for publisher ${publisherId}:`, event);
    delete this.publisherPhotos[publisherId];
  }

  getTotalBooks(): number {
    return this.totalBooks;
  }

  getPublisherBookCount(publisherId: number): number {
    return this.publisherBookCounts[publisherId] || 0;
  }

  // Search and filter methods
  onSearchInput(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.applyFilters();
    }, 300);
  }

  performSearch(): void {
    this.applyFilters();
  }

  // Filter expansion methods
  expandFilters(): void {
    this.isFilterExpanded = true;
  }

  collapseFilters(): void {
    this.isFilterExpanded = false;
  }

  // New backend filtering methods
  onFilterChange(): void {
    // Debounce filter changes
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    
    this.searchTimeout = setTimeout(() => {
      this.applyLocalFilters();
    }, 500);
  }

  applyBackendFilters(): void {
    this.applyLocalFilters();
  }

  clearAllFilters(): void {
    this.filters = {
      name: '',
      description: '',
      status: '',
      minBooks: undefined,
      maxBooks: undefined,
      foundedYear: undefined,
      location: '',
      sortBy: 'name'
    };
    this.applyLocalFilters();
  }

  hasActiveFilters(): boolean {
    return !!(this.filters.name?.trim() || 
              this.filters.description?.trim() || 
              this.filters.status || 
              this.filters.minBooks !== undefined || 
              this.filters.maxBooks !== undefined ||
              this.filters.foundedYear ||
              this.filters.location?.trim());
  }

  private applyFilters(): void {
    let filtered = [...this.publishers];

    // Apply search filter
    if (this.searchTerm.trim() !== '') {
      const searchLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(publisher => 
        publisher.name.toLowerCase().includes(searchLower) ||
        (publisher.description && publisher.description.toLowerCase().includes(searchLower))
      );
    }

    // Apply status filter
    if (this.selectedFilter !== 'all') {
      filtered = filtered.filter(publisher => {
        const bookCount = this.publisherBookCounts[publisher.publisherId] || 0;
        if (this.selectedFilter === 'hasBooks') {
          return bookCount > 0;
        } else if (this.selectedFilter === 'noBooks') {
          return bookCount === 0;
        }
        return true;
      });
    }

    // Apply sorting
    filtered.sort((a, b) => {
      const bookCountA = this.publisherBookCounts[a.publisherId] || 0;
      const bookCountB = this.publisherBookCounts[b.publisherId] || 0;

      switch (this.selectedSort) {
        case 'name':
          return a.name.localeCompare(b.name);
        case 'books':
          return bookCountB - bookCountA;
        default:
          return 0;
      }
    });

    this.filteredPublishers = filtered;
  }

  private applyLocalFilters(): void {
    let filtered = [...this.publishers];

    // Apply name filter
    if (this.filters.name?.trim()) {
      const nameLower = this.filters.name.toLowerCase();
      filtered = filtered.filter(publisher => 
        publisher.name.toLowerCase().includes(nameLower)
      );
    }

    // Apply description filter
    if (this.filters.description?.trim()) {
      const descLower = this.filters.description.toLowerCase();
      filtered = filtered.filter(publisher => 
        publisher.description && publisher.description.toLowerCase().includes(descLower)
      );
    }

    // Apply status filter
    if (this.filters.status) {
      filtered = filtered.filter(publisher => {
        const bookCount = this.publisherBookCounts[publisher.publisherId] || 0;
        if (this.filters.status === 'hasBooks') {
          return bookCount > 0;
        } else if (this.filters.status === 'noBooks') {
          return bookCount === 0;
        }
        return true;
      });
    }

    // Apply books count filter
    if (this.filters.minBooks !== undefined) {
      filtered = filtered.filter(publisher => {
        const bookCount = this.publisherBookCounts[publisher.publisherId] || 0;
        return bookCount >= this.filters.minBooks;
      });
    }

    if (this.filters.maxBooks !== undefined) {
      filtered = filtered.filter(publisher => {
        const bookCount = this.publisherBookCounts[publisher.publisherId] || 0;
        return bookCount <= this.filters.maxBooks;
      });
    }

    // Apply founded year filter
    if (this.filters.foundedYear) {
      filtered = filtered.filter(publisher => 
        publisher.foundedYear && publisher.foundedYear === this.filters.foundedYear
      );
    }

    // Apply location filter
    if (this.filters.location?.trim()) {
      const locationLower = this.filters.location.toLowerCase();
      filtered = filtered.filter(publisher => 
        publisher.location && publisher.location.toLowerCase().includes(locationLower)
      );
    }

    // Apply sorting
    if (this.filters.sortBy) {
      filtered.sort((a, b) => {
        const bookCountA = this.publisherBookCounts[a.publisherId] || 0;
        const bookCountB = this.publisherBookCounts[b.publisherId] || 0;

        switch (this.filters.sortBy) {
          case 'name':
            return a.name.localeCompare(b.name);
          case 'books':
            return bookCountB - bookCountA;
          case 'foundedYear':
            if (!a.foundedYear && !b.foundedYear) return 0;
            if (!a.foundedYear) return 1;
            if (!b.foundedYear) return -1;
            return a.foundedYear - b.foundedYear;
          default:
            return 0;
        }
      });
    }

    this.filteredPublishers = filtered;
  }

  goToPublisherDetail(publisherId: number): void {
    this.router.navigate(['/tr/publisher', publisherId]);
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  retry(): void {
    this.loadPublishers();
  }

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message) => {
      this.error = message;
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    
    // Clean up blob URLs
    Object.values(this.publisherPhotos).forEach(url => {
      if (typeof url === 'string') {
        URL.revokeObjectURL(url);
      }
    });
  }
} 