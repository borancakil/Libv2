import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subscription, forkJoin } from 'rxjs';
import { Author } from '../models/author.model';
import { AuthorApiService } from '../services/author-api';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-author-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, FormsModule],
  templateUrl: './author-list.component.html',
  styleUrls: ['./author-list.component.css'],
})
export class AuthorListComponent implements OnInit, OnDestroy {
  filter: string = '';
  authors: Author[] = [];
  filteredAuthors: Author[] = [];
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];
  
  // Search properties
  searchTerm = '';
  selectedFilter = 'all';
  private searchTimeout: any;

  // Filter properties
  filters: any = {
    name: '',
    biography: '',
    status: '',
    minBooks: undefined,
    maxBooks: undefined,
    birthYear: undefined,
    deathYear: undefined,
    sortBy: 'name'
  };
  
  // Filter expansion state
  isFilterExpanded = false;

  // Photo properties
  authorPhotos: Map<number, SafeUrl> = new Map();
  loadingPhotos: Set<number> = new Set();

  // Book count properties
  authorBookCounts: Map<number, number> = new Map();

  constructor(
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

    this.loadAuthors();
  }

  private loadAuthors(): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.authorApi.getAll(this.filter).subscribe({
      next: (authors) => {
        this.authors = authors;
        this.filteredAuthors = authors; // Initialize filtered authors
        this.isLoading = false;
        this.loadAuthorPhotos();
        this.loadBookCounts();
      },
      error: (err) => {
        console.error('Error loading authors:', err);
        this.isLoading = false;
        this.handleError('AUTHORS_LOAD_ERROR');
      },
    });

    this.subscriptions.push(subscription);
  }

  onFilterButtonClick(): void {
    this.filter = (this.filter || '').trim();
    this.loadAuthors();
  }

  private loadBookCounts(): void {
    if (this.authors.length === 0) return;

    const bookCountRequests = this.authors.map(author => 
      this.authorApi.getBookCount(author.authorId)
    );

    const subscription = forkJoin(bookCountRequests).subscribe({
      next: (counts) => {
        this.authors.forEach((author, index) => {
          this.authorBookCounts.set(author.authorId, counts[index]);
        });
      },
      error: (err) => {
        console.error('Error loading book counts:', err);
        // If book count endpoint doesn't exist, set all counts to 0
        this.authors.forEach(author => {
          this.authorBookCounts.set(author.authorId, 0);
        });
      }
    });

    this.subscriptions.push(subscription);
  }

  private loadAuthorPhotos(): void {
    this.authors.forEach(author => {
      if (author.hasProfileImage) {
        this.loadAuthorPhoto(author.authorId);
      }
    });
  }

  private loadAuthorPhoto(authorId: number): void {
    if (this.loadingPhotos.has(authorId)) {
      return; // Already loading
    }

    this.loadingPhotos.add(authorId);

    const subscription = this.authorApi.getProfileImage(authorId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const safeUrl = this.sanitizer.bypassSecurityTrustUrl(url);
        this.authorPhotos.set(authorId, safeUrl);
        this.loadingPhotos.delete(authorId);
      },
      error: (err) => {
        console.error(`Error loading photo for author ${authorId}:`, err);
        this.loadingPhotos.delete(authorId);
      }
    });

    this.subscriptions.push(subscription);
  }

  getAuthorPhoto(authorId: number): SafeUrl | null {
    return this.authorPhotos.get(authorId) || null;
  }

  isPhotoLoading(authorId: number): boolean {
    return this.loadingPhotos.has(authorId);
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
    const placeholder = img.parentElement?.querySelector('.photo-placeholder') as HTMLElement;
    if (placeholder) {
      placeholder.style.display = 'flex';
    }
  }

  // Search methods
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
      biography: '',
      status: '',
      minBooks: undefined,
      maxBooks: undefined,
      birthYear: undefined,
      deathYear: undefined,
      sortBy: 'name'
    };
    this.applyLocalFilters();
  }

  hasActiveFilters(): boolean {
    return !!(this.filters.name?.trim() || 
              this.filters.biography?.trim() || 
              this.filters.status || 
              this.filters.minBooks !== undefined || 
              this.filters.maxBooks !== undefined ||
              this.filters.birthYear ||
              this.filters.deathYear);
  }

  private applyFilters(): void {
    let filtered = [...this.authors];

    // Apply search filter
    if (this.searchTerm.trim() !== '') {
      const searchLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(author => 
        author.name.toLowerCase().includes(searchLower) ||
        (author.biography && author.biography.toLowerCase().includes(searchLower))
      );
    }

    // Apply status filter
    if (this.selectedFilter !== 'all') {
      filtered = filtered.filter(author => {
        if (this.selectedFilter === 'alive') {
          return !author.deathDate;
        } else if (this.selectedFilter === 'deceased') {
          return !!author.deathDate;
        }
        return true;
      });
    }

    this.filteredAuthors = filtered;
  }

  private applyLocalFilters(): void {
    let filtered = [...this.authors];

    // Apply name filter
    if (this.filters.name?.trim()) {
      const nameLower = this.filters.name.toLowerCase();
      filtered = filtered.filter(author => 
        author.name.toLowerCase().includes(nameLower)
      );
    }

    // Apply biography filter
    if (this.filters.biography?.trim()) {
      const bioLower = this.filters.biography.toLowerCase();
      filtered = filtered.filter(author => 
        author.biography && author.biography.toLowerCase().includes(bioLower)
      );
    }

    // Apply status filter
    if (this.filters.status) {
      filtered = filtered.filter(author => {
        if (this.filters.status === 'alive') {
          return !author.deathDate;
        } else if (this.filters.status === 'deceased') {
          return !!author.deathDate;
        }
        return true;
      });
    }

    // Apply books count filter
    if (this.filters.minBooks !== undefined) {
      filtered = filtered.filter(author => 
        (this.authorBookCounts.get(author.authorId) || 0) >= this.filters.minBooks
      );
    }

    if (this.filters.maxBooks !== undefined) {
      filtered = filtered.filter(author => 
        (this.authorBookCounts.get(author.authorId) || 0) <= this.filters.maxBooks
      );
    }

    // Apply birth year filter
    if (this.filters.birthYear) {
      filtered = filtered.filter(author => 
        author.birthDate && new Date(author.birthDate).getFullYear() === this.filters.birthYear
      );
    }

    // Apply death year filter
    if (this.filters.deathYear) {
      filtered = filtered.filter(author => 
        author.deathDate && new Date(author.deathDate).getFullYear() === this.filters.deathYear
      );
    }

    // Apply sorting
    if (this.filters.sortBy) {
      filtered.sort((a, b) => {
        switch (this.filters.sortBy) {
          case 'name':
            return a.name.localeCompare(b.name);
          case 'books':
            return (this.authorBookCounts.get(b.authorId) || 0) - (this.authorBookCounts.get(a.authorId) || 0);
          case 'birthDate':
            if (!a.birthDate && !b.birthDate) return 0;
            if (!a.birthDate) return 1;
            if (!b.birthDate) return -1;
            return new Date(a.birthDate).getTime() - new Date(b.birthDate).getTime();
          default:
            return 0;
        }
      });
    }

    this.filteredAuthors = filtered;
  }

  getTotalBooksCount(): number {
    return this.filteredAuthors.reduce((total, author) => {
      return total + (this.authorBookCounts.get(author.authorId) || 0);
    }, 0);
  }

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang]);
  }

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message: string) => {
      this.error = message;
    });
    this.isLoading = false;
  }

  goToAuthorDetail(authorId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'author', authorId]);
  }

  retry(): void {
    this.loadAuthors();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    // Clean up blob URLs
    this.authorPhotos.forEach(url => {
      if (typeof url === 'string') {
        URL.revokeObjectURL(url);
      }
    });
  }
} 