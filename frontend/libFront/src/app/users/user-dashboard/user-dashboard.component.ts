import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Book } from '../../books/models/book.model';
import { UserApiService } from '../services/user-api';
import { AuthService } from '../../services/auth.service';
import { BookApiService } from '../../books/services/book-api';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './user-dashboard.component.html',
  styleUrls: ['./user-dashboard.component.css']
})
export class UserDashboardComponent implements OnInit, OnDestroy {
  activeTab: 'favorites' | 'borrowed' = 'favorites';
  favoriteBooks: Book[] = [];
  borrowedBooks: Book[] = [];
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];
  private retryCount = 0;
  private maxRetries = 3;

  constructor(
    public router: Router,
    private userApi: UserApiService,
    private bookApi: BookApiService,
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

    // Authentication durumunu kontrol et
    if (!this.checkIfUserLoggedIn()) {
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }

    // Cookie'lerin set edilmesi için kısa bir delay ekle
    setTimeout(() => {
      this.loadData();
    }, 200);

    // Authentication state değişikliklerini dinle
    window.addEventListener('authStateChanged', this.handleAuthStateChange);
  }

  private loadData(): void {
    // Authentication durumunu tekrar kontrol et
    if (!this.checkIfUserLoggedIn()) {
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }

    // Cookie'lerin hazır olması için biraz daha bekle
    if (!this.auth.isLoggedIn()) {
      if (this.retryCount < this.maxRetries) {
        this.retryCount++;
        console.log(`Authentication not ready, retrying ${this.retryCount}/${this.maxRetries} in 500ms...`);
        setTimeout(() => {
          this.loadData();
        }, 500);
        return;
      } else {
        console.log('Max retries reached, redirecting to login');
        const lang = this.translate.currentLang || 'tr';
        this.router.navigate(['/', lang, 'login']);
        return;
      }
    }

    // Reset retry count on successful authentication check
    this.retryCount = 0;

    this.isLoading = true;
    this.error = null;

    console.log('Loading favorites and borrowed books...');

    // Load both favorites and borrowed books
    const favoritesSub = this.userApi.getMyFavoriteBooks().subscribe({
      next: (books) => {
        console.log('Favorites loaded:', books.length);
        this.favoriteBooks = books;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading favorites:', err);
        if (err.status === 401) {
          // Unauthorized - kullanıcıyı login'e yönlendir
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate(['/', lang, 'login']);
        } else {
          this.handleError('FAVORITES_LOAD_ERROR');
        }
      }
    });

    const borrowedSub = this.userApi.getMyBorrowedBooks().subscribe({
      next: (books) => {
        console.log('Borrowed books loaded:', books.length);
        this.borrowedBooks = books;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading borrowed books:', err);
        if (err.status === 401) {
          // Unauthorized - kullanıcıyı login'e yönlendir
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate(['/', lang, 'login']);
        } else {
          this.handleError('BORROWED_BOOKS_LOAD_ERROR');
        }
      }
    });

    this.subscriptions.push(favoritesSub, borrowedSub);
  }

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message: string) => {
      this.error = message;
      this.toastService.error(message);
    });
    this.isLoading = false;
  }

  setActiveTab(tab: 'favorites' | 'borrowed'): void {
    this.activeTab = tab;
  }

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang]);
  }

  goToBookDetail(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'book', bookId]);
  }

  removeFromFavorites(bookId: number): void {
    const subscription = this.userApi.removeFromMyFavorites(bookId).subscribe({
      next: () => {
        this.translate.get('REMOVED_FROM_FAVORITES').subscribe((msg: string) => {
          this.toastService.success(msg);
        });
        // Remove from local array
        this.favoriteBooks = this.favoriteBooks.filter(book => book.bookId !== bookId);
      },
      error: (err) => {
        this.translate.get('REMOVE_FROM_FAVORITES_ERROR').subscribe((msg: string) => {
          this.toastService.error(msg);
        });
      }
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
      next: () => {
        this.translate.get('RETURN_SUCCESS').subscribe((msg: string) => {
          this.toastService.success(msg);
        });
        // Remove from local array
        this.borrowedBooks = this.borrowedBooks.filter(book => book.bookId !== bookId);
      },
      error: (err) => {
        this.translate.get('RETURN_ERROR').subscribe((msg: string) => {
          this.toastService.error(msg);
        });
      }
    });
    this.subscriptions.push(subscription);
  }

  renewBook(bookId: number): void {
    // TODO: Implement renew functionality
    this.translate.get('RENEW_FEATURE_COMING_SOON').subscribe((msg: string) => {
      this.toastService.info(msg);
    });
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
      try {
        const user = JSON.parse(userStr);
        return user.userId;
      } catch (e) {
        console.error('Error parsing user from localStorage', e);
        return 1;
      }
    }
    return 1;
  }

  getBookCoverUrl(bookId: number): string {
    return `https://localhost:7209/api/Books/${bookId}/cover`;
  }

  getRemainingDays(returnDate: string): number {
    const today = new Date();
    const returnDateObj = new Date(returnDate);
    const diffTime = returnDateObj.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }

  isOverdue(returnDate: string): boolean {
    return this.getRemainingDays(returnDate) < 0;
  }

  isWarning(returnDate: string): boolean {
    const remainingDays = this.getRemainingDays(returnDate);
    return remainingDays >= 0 && remainingDays <= 3;
  }

  retry(): void {
    // Authentication durumunu tekrar kontrol et
    if (!this.checkIfUserLoggedIn()) {
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }
    
    // Reset retry count and load data
    this.retryCount = 0;
    this.loadData();
  }

  hideImage(event: Event): void {
    const imgElement = event.target as HTMLImageElement;
    imgElement.style.display = 'none';
    const parent = imgElement.closest('.book-cover');
    if (parent) {
      const placeholder = parent.querySelector('.cover-placeholder') as HTMLElement;
      if (placeholder) {
        placeholder.style.display = 'flex';
      }
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
    
    // Event listener'ı temizle
    if (this.isBrowser) {
      window.removeEventListener('authStateChanged', this.handleAuthStateChange);
    }
  }

  private handleAuthStateChange = (event: any) => {
    if (!event.detail.isLoggedIn) {
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
    }
  };
} 