import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Book, BookStatusForUser, BookCategory, BookFilterDto } from '../models/book.model';
import { BookApiService } from '../services/book-api';
import { AuthorApiService } from '../services/author-api';
import { PublisherApiService } from '../services/publisher-api';
import { UserApiService } from '../../users/services/user-api';
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
  books: Book[] = [];
  filteredBooks: Book[] = [];
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

  constructor(
    public router: Router,
    private bookApi: BookApiService,
    private authorApi: AuthorApiService,
    private publisherApi: PublisherApiService,
    private userApi: UserApiService,
    public translate: TranslateService,
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
        this.isLoading = false;
        this.loadBookStatuses();
      },
      error: (err) => {
        console.error('Error loading books:', err);
        this.isLoading = false;
        this.handleError('BOOKS_LOAD_ERROR');
      },
    });

    this.subscriptions.push(subscription);
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
        this.filteredBooks = books;
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

  private loadBookStatuses(): void {
    if (!this.checkIfUserLoggedIn()) {
      return;
    }

    const userId = this.getCurrentUserId();

    this.books.forEach((book) => {
      const subscription = this.bookApi
        .getBookStatusForUser(book.bookId, userId)
        .subscribe({
          next: (status) => {
            this.bookStatuses.set(book.bookId, status);
          },
          error: (err) => {
            console.error(`Error loading status for book ${book.bookId}:`, err);
          },
        });
      this.subscriptions.push(subscription);
    });
  }

  private prepareDropdownData(): void {
    // Extract unique authors and publishers from books
    const authors = new Set<string>();
    const publishers = new Set<string>();

    this.books.forEach((book) => {
      if (book.author && book.author.name) {
        authors.add(book.author.name);
      }
      if (book.publisher && book.publisher.name) {
        publishers.add(book.publisher.name);
      }
    });

    this.availableAuthors = Array.from(authors).sort();
    this.availablePublishers = Array.from(publishers).sort();
  }

  private checkIfUserLoggedIn(): boolean {
    if (!this.isBrowser) {
      return false;
    }

    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('user');

    return !!(token && user);
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

  borrowBook(bookId: number): void {
    if (!this.checkIfUserLoggedIn()) {
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }

    const userId = this.getCurrentUserId();
    const borrowDto = {
      bookId: bookId,
      userId: userId,
      borrowDate: new Date().toISOString(),
      returnDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString(),
    };

    const subscription = this.bookApi.borrow(bookId, borrowDto).subscribe({
      next: (response) => {
        this.translate.get('BORROW_SUCCESS').subscribe((msg: string) => {
          this.toastService.success(msg);
        });
        this.loadBookStatuses();
      },
      error: (err) => {
        this.translate.get('BORROW_ERROR').subscribe((msg: string) => {
          this.toastService.error(msg);
        });
      },
    });
    this.subscriptions.push(subscription);
  }

  returnBook(bookId: number): void {
    if (!this.checkIfUserLoggedIn()) {
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }

    const userId = this.getCurrentUserId();

    const subscription = this.bookApi.return(bookId, userId).subscribe({
      next: (response) => {
        this.translate.get('RETURN_SUCCESS').subscribe((msg: string) => {
          this.toastService.success(msg);
        });
        this.loadBookStatuses();
      },
      error: (err) => {
        this.translate.get('RETURN_ERROR').subscribe((msg: string) => {
          this.toastService.error(msg);
        });
      },
    });
    this.subscriptions.push(subscription);
  }

  isBorrowing(bookId: number): boolean {
    return false; // TODO: Implement loading state tracking
  }

  isReturning(bookId: number): boolean {
    return false; // TODO: Implement loading state tracking
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}
