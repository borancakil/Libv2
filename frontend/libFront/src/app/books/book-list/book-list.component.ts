import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Book, BookListDto, BookStatusForUser, BookCategory, BookFilterDto } from '../models/book.model';
import { BookApiService } from '../services/book-api';
import { AuthorApiService } from '../services/author-api';
import { PublisherApiService } from '../services/publisher-api';
import { UserApiService } from '../../users/services/user-api';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-book-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, FormsModule],
  templateUrl: './book-list.component.html',
  styleUrls: ['./book-list.component.css'],
})
export class BookListComponent implements OnInit, OnDestroy {
  filter: string = '';
  books: BookListDto[] = [];
  filteredBooks: BookListDto[] = [];
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];
  bookStatuses: Map<number, BookStatusForUser> = new Map();

  // Filter properties
  filters: BookFilterDto = {
    title: '',
    author: '',
    publisher: '',
    year: undefined,
    isAvailable: undefined,
    categories: [],
    minRating: undefined,
    maxRating: undefined,
  };

  // Filter expansion state
  isFilterExpanded = false;

  // Dropdown states
  isAuthorDropdownOpen = false;
  isPublisherDropdownOpen = false;
  isCategoryDropdownOpen = false;

  // Dropdown data
  availableAuthors: string[] = [];
  availablePublishers: string[] = [];
  availableCategories: { value: BookCategory; label: string }[] = [];

  // Legacy search properties (for backward compatibility)
  searchTerm = '';
  selectedFilter = 'all';
  private searchTimeout: any;

  // Pagination properties
  currentPage = 1;
  pageSize = 9;
  totalBooks = 0;
  totalPages = 0;
  displayedBooks: BookListDto[] = [];

  constructor(
    public router: Router,
    private bookApi: BookApiService,
    private authorApi: AuthorApiService,
    private publisherApi: PublisherApiService,
    private userApi: UserApiService,
    public translate: TranslateService,
    private auth: AuthService,
    private toastService: ToastService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser) {
      return;
    }

    this.loadBooks();
    this.loadDropdownData();
    this.initializeCategories();
  }

  private loadBooks(): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.bookApi.getAll(this.filter).subscribe({
      next: (books) => {
        this.books = books;
        this.filteredBooks = books; // Initialize filtered books
        this.totalBooks = books.length;
        this.totalPages = Math.ceil(this.totalBooks / this.pageSize);
        this.updateDisplayedBooks();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading books:', err);
        this.isLoading = false;
        this.handleError('BOOKS_LOAD_ERROR');
      },
    });

    this.subscriptions.push(subscription);
  }

  private updateDisplayedBooks(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedBooks = this.filteredBooks.slice(startIndex, endIndex);
  }

  // Pagination methods
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updateDisplayedBooks();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    
    if (this.totalPages <= maxVisiblePages) {
      // Show all pages if total is small
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Show pages around current page
      let start = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
      let end = Math.min(this.totalPages, start + maxVisiblePages - 1);
      
      // Adjust start if we're near the end
      if (end - start < maxVisiblePages - 1) {
        start = Math.max(1, end - maxVisiblePages + 1);
      }
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
    }
    
    return pages;
  }

  // Helper method for template
  getMath(): any {
    return Math;
  }

  onFilterButtonClick(): void {
    this.filter = (this.filter || '').trim();
    this.loadBooks();
  }

  private loadDropdownData(): void {
    // Load authors from backend
    const authorsSubscription = this.authorApi.getAll().subscribe({
      next: (authors) => {
        this.availableAuthors = authors.map((author) => author.name).sort();
      },
      error: (err) => {
        console.error('Error loading authors:', err);
        this.availableAuthors = [];
      },
    });

    // Load publishers from backend
    const publishersSubscription = this.publisherApi.getAll().subscribe({
      next: (publishers) => {
        this.availablePublishers = publishers
          .map((publisher) => publisher.name)
          .sort();
      },
      error: (err) => {
        console.error('Error loading publishers:', err);
        this.availablePublishers = [];
      },
    });

    this.subscriptions.push(authorsSubscription, publishersSubscription);
  }

  private initializeCategories(): void {
    this.availableCategories = [
      { value: BookCategory.Fiction, label: 'FICTION' },
      { value: BookCategory.NonFiction, label: 'NON_FICTION' },
      { value: BookCategory.Science, label: 'SCIENCE' },
      { value: BookCategory.History, label: 'HISTORY' },
      { value: BookCategory.Philosophy, label: 'PHILOSOPHY' },
      { value: BookCategory.Technology, label: 'TECHNOLOGY' },
      { value: BookCategory.Art, label: 'ART' },
      { value: BookCategory.Literature, label: 'LITERATURE' },
      { value: BookCategory.Biography, label: 'BIOGRAPHY' },
      { value: BookCategory.Children, label: 'CHILDREN' },
    ];
  }

  // Apply filters button method
  applyFilters(): void {
    this.applyBackendFilters();
  }

  // New backend filtering methods
  onFilterChange(): void {
    // Remove automatic filtering - now only applies when button is clicked
    // Debounce is removed since we're using a button
  }

  applyBackendFilters(): void {
    // Construct clean filter object (remove empty/undefined values)
    const cleanFilter: BookFilterDto = {};

    if (this.filters.title && this.filters.title.trim()) {
      cleanFilter.title = this.filters.title.trim();
    }
    if (this.filters.author && this.filters.author.trim()) {
      cleanFilter.author = this.filters.author.trim();
    }
    if (this.filters.publisher && this.filters.publisher.trim()) {
      cleanFilter.publisher = this.filters.publisher.trim();
    }
    if (this.filters.year) {
      cleanFilter.year = this.filters.year;
    }
    if (this.filters.isAvailable !== undefined) {
      cleanFilter.isAvailable = this.filters.isAvailable;
    }
    if (this.filters.categories && this.filters.categories.length > 0) {
      cleanFilter.categories = this.filters.categories;
    }
    if (this.filters.minRating) {
      cleanFilter.minRating = this.filters.minRating;
    }
    if (this.filters.maxRating) {
      cleanFilter.maxRating = this.filters.maxRating;
    }

    this.isLoading = true;
    this.error = null;

    const subscription = this.bookApi.filterBooks(cleanFilter).subscribe({
      next: (books) => {
        // Convert Book[] to BookListDto[] for compatibility
        this.filteredBooks = books.map(book => ({
          bookId: book.bookId,
          title: book.title,
          publicationYear: book.publicationYear,
          authorName: book.authorName || '',
          publisherName: book.publisherName || '',
          categoryName: book.categories?.[0]?.toString() || '',
          callNo: book.bookId.toString().padStart(6, '0'),
          isAvailable: book.isAvailable,
          rating: 0
        }));
        this.totalBooks = this.filteredBooks.length;
        this.totalPages = Math.ceil(this.totalBooks / this.pageSize);
        this.currentPage = 1; // Reset to first page when filtering
        this.updateDisplayedBooks();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error filtering books:', err);
        this.isLoading = false;
        this.handleError('FILTER_ERROR');
        // Fallback to local filtering
        this.applyLocalFilters();
      },
    });

    this.subscriptions.push(subscription);
  }

  clearAllFilters(): void {
    this.filters = {
      title: '',
      author: '',
      publisher: '',
      year: undefined,
      isAvailable: undefined,
      categories: [],
      minRating: undefined,
      maxRating: undefined,
    };
    this.applyBackendFilters();
  }

  hasActiveFilters(): boolean {
    return !!(
      this.filters.title ||
      this.filters.author ||
      this.filters.publisher ||
      this.filters.year ||
      this.filters.isAvailable !== undefined ||
      (this.filters.categories && this.filters.categories.length > 0) ||
      this.filters.minRating ||
      this.filters.maxRating
    );
  }

  expandFilters(): void {
    this.isFilterExpanded = true;
  }

  collapseFilters(): void {
    this.isFilterExpanded = false;
  }

  // Dropdown methods
  toggleAuthorDropdown(): void {
    this.isAuthorDropdownOpen = !this.isAuthorDropdownOpen;
    this.isPublisherDropdownOpen = false; // Close other dropdowns
    this.isCategoryDropdownOpen = false;
  }

  togglePublisherDropdown(): void {
    this.isPublisherDropdownOpen = !this.isPublisherDropdownOpen;
    this.isAuthorDropdownOpen = false; // Close other dropdowns
    this.isCategoryDropdownOpen = false;
  }

  toggleCategoryDropdown(): void {
    this.isCategoryDropdownOpen = !this.isCategoryDropdownOpen;
    this.isAuthorDropdownOpen = false; // Close other dropdowns
    this.isPublisherDropdownOpen = false;
  }

  selectAuthor(author: string): void {
    this.filters.author = author;
    this.isAuthorDropdownOpen = false;
  }

  selectPublisher(publisher: string): void {
    this.filters.publisher = publisher;
    this.isPublisherDropdownOpen = false;
  }

  selectCategory(category: BookCategory): void {
    if (!this.filters.categories) {
      this.filters.categories = [];
    }

    const index = this.filters.categories.indexOf(category);
    if (index > -1) {
      this.filters.categories.splice(index, 1);
    } else {
      this.filters.categories.push(category);
    }
  }

  clearAuthorFilter(): void {
    this.filters.author = '';
    this.isAuthorDropdownOpen = false;
  }

  clearPublisherFilter(): void {
    this.filters.publisher = '';
    this.isPublisherDropdownOpen = false;
  }

  clearCategoryFilter(): void {
    this.filters.categories = [];
    this.isCategoryDropdownOpen = false;
  }

  isCategorySelected(category: BookCategory): boolean {
    return this.filters.categories?.includes(category) || false;
  }

  getSelectedCategoriesText(): string {
    if (!this.filters.categories || this.filters.categories.length === 0) {
      return '';
    }

    const selectedLabels = this.filters.categories
      .map((cat) => {
        const category = this.availableCategories.find((c) => c.value === cat);
        return category ? this.translate.instant(category.label) : '';
      })
      .filter((label) => label);

    return selectedLabels.join(', ');
  }

  // Legacy methods (for backward compatibility)
  onSearchInput(): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }

    this.searchTimeout = setTimeout(() => {
      this.applyLocalFilters();
    }, 300);
  }

  performSearch(): void {
    this.applyLocalFilters();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedFilter = 'all';
    this.applyLocalFilters();
  }

  private applyLocalFilters(): void {
    let filtered = [...this.books];

    // Apply search filter
    if (this.searchTerm.trim() !== '') {
      const searchLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(
        (book) =>
          book.title.toLowerCase().includes(searchLower) ||
          (book.authorName &&
            book.authorName.toLowerCase().includes(searchLower)) ||
          (book.publisherName &&
            book.publisherName.toLowerCase().includes(searchLower)) ||
          book.bookId.toString().includes(searchLower)
      );
    }

    // Apply availability filter
    if (this.selectedFilter !== 'all') {
      filtered = filtered.filter((book) => {
        if (this.selectedFilter === 'available') {
          return book.isAvailable;
        } else if (this.selectedFilter === 'unavailable') {
          return !book.isAvailable;
        }
        return true;
      });
    }

    this.filteredBooks = filtered;
  }



  private prepareDropdownData(): void {
    // Extract unique authors and publishers from books
    const authors = new Set<string>();
    const publishers = new Set<string>();

    this.books.forEach((book) => {
      if (book.authorName) {
        authors.add(book.authorName);
      }
      if (book.publisherName) {
        publishers.add(book.publisherName);
      }
    });

    this.availableAuthors = Array.from(authors).sort();
    this.availablePublishers = Array.from(publishers).sort();
  }

  private checkIfUserLoggedIn(): boolean {
    if (!this.isBrowser) return false;
    return this.auth.isLoggedIn();
  }

  private getCurrentUserId(): number {
    if (!this.isBrowser) {
      return 1;
    }

    const userStr = localStorage.getItem('user');
    if (userStr) {
      const user = JSON.parse(userStr);
      return user.userId || 1;
    }
    return 1;
  }

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message: string) => {
      this.error = message;
    });
    this.isLoading = false;
  }

  goToBookDetail(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'book', bookId]);
  }

  goToAuthorDetail(authorId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'author', authorId]);
  }

  goToPublisherDetail(publisherId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'publisher', publisherId]);
  }

  getBookStatus(bookId: number): BookStatusForUser | null {
    return this.bookStatuses.get(bookId) || null;
  }

  hideImage(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.style.display = 'none';
    }
  }

  retry(): void {
    this.loadBooks();
  }

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang]);
  }

  getAvailableBooksCount(): number {
    return this.books.filter((book) => book.isAvailable).length;
  }



  ngOnDestroy(): void {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}
