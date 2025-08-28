import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { Book } from '../../books/models/book.model';
import { UserApiService } from '../services/user-api';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './user-dashboard.component.html',
  styleUrls: ['./user-dashboard.component.css']
})
export class UserDashboardComponent implements OnInit, OnDestroy {
  activeTab: 'favorites' | 'borrowed' = 'favorites';
  viewMode: 'grid' | 'list' = 'grid';
  favoriteBooks: Book[] = [];
  borrowedBooks: Book[] = [];
  isLoading = false;
  error: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private userApi: UserApiService,
    private router: Router,
    private translate: TranslateService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadUserData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUserData(): void {
    this.isLoading = true;
    this.error = null;

    // Load favorites
    this.userApi.getMyFavoriteBooks()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (books: Book[]) => {
          this.favoriteBooks = books;
        },
        error: (error: any) => {
          console.error('Error loading favorites:', error);
          this.error = 'Error loading favorites';
        }
      });

    // Load borrowed books
    this.userApi.getMyBorrowedBooks()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (books: Book[]) => {
          this.borrowedBooks = books;
          this.isLoading = false;
        },
        error: (error: any) => {
          console.error('Error loading borrowed books:', error);
          this.error = 'Error loading borrowed books';
          this.isLoading = false;
        }
      });
  }

  setActiveTab(tab: 'favorites' | 'borrowed'): void {
    this.activeTab = tab;
  }

  setViewMode(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  }

  goToBookDetail(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'books', bookId]);
  }

  removeFromFavorites(bookId: number): void {
    this.userApi.removeFromMyFavorites(bookId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.favoriteBooks = this.favoriteBooks.filter(book => book.bookId !== bookId);
          this.toastService.success('Book removed from favorites');
        },
        error: (error: any) => {
          console.error('Error removing from favorites:', error);
          this.toastService.error('Error removing from favorites');
        }
      });
  }

  returnBook(bookId: number): void {
    // For now, we'll just remove from borrowed books
    // In a real implementation, you'd call an API to return the book
    this.borrowedBooks = this.borrowedBooks.filter(book => book.bookId !== bookId);
    this.toastService.success('Book returned successfully');
  }

  getBookCoverUrl(bookId: number): string {
    return `/api/books/${bookId}/cover`;
  }

  hideImage(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
  }

  getRemainingDays(returnDate: string | undefined): number {
    if (!returnDate) return 0;
    const today = new Date();
    const returnDateObj = new Date(returnDate);
    const diffTime = returnDateObj.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }

  isOverdue(returnDate: string | undefined): boolean {
    return this.getRemainingDays(returnDate) < 0;
  }

  isWarning(returnDate: string | undefined): boolean {
    const remainingDays = this.getRemainingDays(returnDate);
    return remainingDays >= 0 && remainingDays <= 3;
  }

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang]);
  }

  goToProfile(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'profile']);
  }

  retry(): void {
    this.loadUserData();
  }
} 