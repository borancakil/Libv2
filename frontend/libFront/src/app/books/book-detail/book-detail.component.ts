import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Book, BookStatusForUser, FavoriteStatus } from '../models/book.model';
import { BookApiService } from '../services/book-api';
import { UserApiService } from '../../users/services/user-api';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-book-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './book-detail.component.html',
  styleUrls: ['./book-detail.component.css'],
})
export class BookDetailComponent implements OnInit, OnDestroy {
  book: Book | null = null;
  bookStatus: BookStatusForUser | null = null;
  isLoading = true;
  isBorrowing = false;
  isAddingToFavorites = false;
  isInFavorites = false;
  isBorrowedByUser = false;
  coverImageUrl: string | null = null;
  showImageModal = false;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    private bookApi: BookApiService,
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

    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (idParam) {
      const bookId = parseInt(idParam, 10);
      
      if (!isNaN(bookId)) {
        this.loadBook(bookId);
        this.loadBookStatus(bookId);
        this.loadCoverImage(bookId);
        this.checkIfInFavorites(bookId); // Favori durumunu kontrol et
      } else {
        this.handleError('BOOK_DETAIL_INVALID_ID');
      }
    } else {
      this.handleError('BOOK_DETAIL_NO_ID');
    }
  }

  private loadBook(id: number): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.bookApi.getById(id).subscribe({
      next: (book) => {
        console.log('Kitap detayı:', book);
        this.book = book;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading book:', err);
        this.isLoading = false;
        
        if (err.status === 404) {
          this.handleError('BOOK_DETAIL_NOT_FOUND');
        } else {
          this.handleError('BOOK_DETAIL_LOAD_ERROR');
        }
      },
    });

    this.subscriptions.push(subscription);
  }

  private loadBookStatus(bookId: number): void {
    if (!this.checkIfUserLoggedIn()) {
      return;
    }

    const userId = this.getCurrentUserId();
    
    const subscription = this.bookApi.getBookStatusForUser(bookId, userId).subscribe({
      next: (status) => {
        this.bookStatus = status;
        this.isBorrowedByUser = status.isBorrowedByUser;
      },
      error: (err) => {
        console.error('Error loading book status:', err);
      }
    });

    this.subscriptions.push(subscription);
  }

  private loadCoverImage(bookId: number): void {
    const subscription = this.bookApi.getCoverImage(bookId).subscribe({
      next: (blob) => {
        this.coverImageUrl = URL.createObjectURL(blob);
      },
      error: (err) => {
        console.error('Error loading cover image:', err);
        // Image not found or error - this is normal for books without covers
      }
    });

    this.subscriptions.push(subscription);
  }

  private checkIfInFavorites(bookId: number): void {
    if (!this.checkIfUserLoggedIn()) {
      this.isInFavorites = false;
      return;
    }
    
    const subscription = this.userApi.isBookInMyFavorites(bookId).subscribe({
      next: (isFavorited: boolean) => {
        this.isInFavorites = isFavorited;
      },
      error: (err) => {
        console.error('Error checking favorites:', err);
        this.isInFavorites = false;
      }
    });

    this.subscriptions.push(subscription);
  }

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message: string) => {
      this.error = message;
      this.toastService.error(message);
    });
    this.isLoading = false;
  }

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'books']);
  }

  goToAuthorDetail(authorId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'author', authorId]);
  }

  goToPublisherDetail(publisherId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'publisher', publisherId]);
  }

  borrowBook(): void {
    const isLoggedIn = this.checkIfUserLoggedIn();
    
    if (!isLoggedIn) {
      console.log('Kullanıcı giriş yapmamış, login sayfasına yönlendiriliyor...');
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }

    if (!this.book) {
      console.log('Kitap bilgisi bulunamadı');
      return;
    }

    if (!this.book.isAvailable) {
      this.translate.get('BOOK_NOT_AVAILABLE').subscribe((msg: string) => {
        this.toastService.warning(msg);
      });
      return;
    }

    this.isBorrowing = true;
    console.log('Ödünç alma işlemi başlatılıyor...');

    const borrowDto = {
      bookId: this.book.bookId,
      userId: this.getCurrentUserId(),
      borrowDate: new Date().toISOString(),
      returnDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString()
    };

    console.log('Borrow DTO:', borrowDto);

    const subscription = this.bookApi.borrow(this.book.bookId, borrowDto).subscribe({
      next: (response) => {
        console.log('Kitap başarıyla ödünç alındı:', response);
        this.isBorrowing = false;
        this.isBorrowedByUser = true;
        this.loadBook(this.book!.bookId);
        this.loadBookStatus(this.book!.bookId);
        this.translate.get('BORROW_SUCCESS').subscribe((msg: string) => {
          this.toastService.success(msg);
        });
      },
      error: (err) => {
        console.error('Ödünç alma hatası:', err);
        this.isBorrowing = false;
        if (err.status === 401) {
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate(['/', lang, 'login']);
        } else {
          this.handleError('BORROW_ERROR');
        }
      }
    });

    this.subscriptions.push(subscription);
  }

  returnBook(): void {
    const isLoggedIn = this.checkIfUserLoggedIn();
    
    if (!isLoggedIn) {
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }

    if (!this.book) {
      return;
    }

    this.isBorrowing = true;
    const userId = this.getCurrentUserId();

    const subscription = this.bookApi.return(this.book.bookId, userId).subscribe({
      next: (response) => {
        console.log('Kitap başarıyla iade edildi:', response);
        this.isBorrowing = false;
        this.isBorrowedByUser = false;
        this.loadBook(this.book!.bookId);
        this.loadBookStatus(this.book!.bookId);
        this.translate.get('RETURN_SUCCESS').subscribe((msg: string) => {
          this.toastService.success(msg);
        });
      },
      error: (err) => {
        console.error('İade etme hatası:', err);
        this.isBorrowing = false;
        if (err.status === 401) {
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate(['/', lang, 'login']);
        } else {
          this.handleError('RETURN_ERROR');
        }
      }
    });

    this.subscriptions.push(subscription);
  }

  toggleFavorite(): void {
    const isLoggedIn = this.checkIfUserLoggedIn();
    
    if (!isLoggedIn) {
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }

    if (!this.book) {
      return;
    }

    this.isAddingToFavorites = true;

    if (this.isInFavorites) {
      const subscription = this.userApi.removeFromMyFavorites(this.book.bookId).subscribe({
        next: () => {
          this.isInFavorites = false;
          this.isAddingToFavorites = false;
          // Favori durumunu tekrar kontrol et
          this.checkIfInFavorites(this.book!.bookId);
          this.translate.get('REMOVED_FROM_FAVORITES').subscribe((msg: string) => {
            this.toastService.success(msg);
          });
        },
        error: (err) => {
          console.error('Favorilerden kaldırma hatası:', err);
          this.isAddingToFavorites = false;
          this.handleError('REMOVE_FAVORITE_ERROR');
        }
      });
      this.subscriptions.push(subscription);
    } else {
      const subscription = this.userApi.addToMyFavorites(this.book.bookId).subscribe({
        next: () => {
          this.isInFavorites = true;
          this.isAddingToFavorites = false;
          // Favori durumunu tekrar kontrol et
          this.checkIfInFavorites(this.book!.bookId);
          this.translate.get('ADDED_TO_FAVORITES').subscribe((msg: string) => {
            this.toastService.success(msg);
          });
        },
        error: (err) => {
          console.error('Favorilere ekleme hatası:', err);
          this.isAddingToFavorites = false;
          this.handleError('ADD_FAVORITE_ERROR');
        }
      });
      this.subscriptions.push(subscription);
    }
  }

  private checkIfUserLoggedIn(): boolean {
    // SSR sırasında localStorage mevcut değil
    if (!this.isBrowser) {
      return false;
    }
    
    // Local storage'dan token kontrolü
    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('user');
    const sessionExpiry = localStorage.getItem('sessionExpiry');
    
    // Token, user ve session expiry kontrolü
    if (token && user && sessionExpiry) {
      try {
        const userObj = JSON.parse(user);
        const expiryTime = parseInt(sessionExpiry, 10);
        const currentTime = Date.now();
        
        // Session süresi dolmuş mu kontrol et
        if (currentTime > expiryTime) {
          return false;
        }
        
        if (userObj && userObj.userId) {
          return true;
        }
      } catch (e) {
        return false;
      }
    }
    return false;
  }

  private getCurrentUserId(): number {
    // SSR sırasında localStorage mevcut değil
    if (!this.isBrowser) {
      return 1;
    }
    
    // Local storage'dan user ID'yi al
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        return user.userId || 1; // Varsayılan olarak 1
      } catch (e) {
        console.error('Error parsing user data:', e);
        return 1;
      }
    }
    return 1; // Varsayılan kullanıcı ID
  }

  retry(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const bookId = parseInt(idParam, 10);
      if (!isNaN(bookId)) {
        this.loadBook(bookId);
        this.loadBookStatus(bookId);
        this.loadCoverImage(bookId);
      }
    }
  }

  hideImage(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.style.display = 'none';
    }
  }

  openImageModal(): void {
    if (this.coverImageUrl) {
      this.showImageModal = true;
    }
  }

  closeImageModal(): void {
    this.showImageModal = false;
  }

  onModalClick(event: Event): void {
    // Modal dışına tıklandığında kapat
    if (event.target === event.currentTarget) {
      this.closeImageModal();
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    if (this.coverImageUrl) {
      URL.revokeObjectURL(this.coverImageUrl);
    }
  }
} 