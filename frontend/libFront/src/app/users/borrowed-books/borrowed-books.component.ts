import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Book } from '../../books/models/book.model';
import { UserApiService } from '../services/user-api';

@Component({
  selector: 'app-borrowed-books',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './borrowed-books.component.html',
  styleUrls: ['./borrowed-books.component.css'],
})
export class BorrowedBooksComponent implements OnInit, OnDestroy {
  borrowedBooks: Book[] = [];
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
    public router: Router,
    private userApi: UserApiService,
    public translate: TranslateService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser) {
      return; // SSR sırasında API çağrısı yapma
    }

    this.loadBorrowedBooks();
  }

  private loadBorrowedBooks(): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.userApi.getMyBorrowedBooks().subscribe({
      next: (books) => {
        this.borrowedBooks = books;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading borrowed books:', err);
        this.isLoading = false;
        
        if (err.status === 401) {
          // Token geçersiz, login sayfasına yönlendir
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate(['/', lang, 'login']);
        } else {
          this.handleError('BORROWED_BOOKS_LOAD_ERROR');
        }
      },
    });

    this.subscriptions.push(subscription);
  }

  private getCurrentUserId(): number {
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

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang]);
  }

  retry(): void {
    this.loadBorrowedBooks();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 