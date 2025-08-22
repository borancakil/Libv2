import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Book, BookStatusForUser, FavoriteStatus } from '../models/book.model';
import { BookApiService } from '../services/book-api';
import { UserApiService } from '../../users/services/user-api';
import { AuthService } from '../../services/auth.service';
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
    // Kullanıcı giriş yapmamışsa bookStatus null kalacak
    if (!this.checkIfUserLoggedIn()) {
      this.bookStatus = null;
      return;
    }

    // LocalStorage'dan user bilgisini al
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      this.bookStatus = null;
      return;
    }
    
    try {
      const user = JSON.parse(userStr);
      const userId = user.userId;
      
      if (!userId) {
        this.bookStatus = null;
        return;
      }
      
      // Book status'u yükle
      const subscription = this.bookApi.getBookStatusForUser(bookId).subscribe({
        next: (status) => {
          this.bookStatus = status;
        },
              error: (err) => {
        this.bookStatus = null;
      }
      });

      this.subscriptions.push(subscription);
    } catch (error) {
      this.bookStatus = null;
    }
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
    
    // LocalStorage'dan user bilgisini al
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      this.isBorrowing = false;
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }
    
    try {
      const user = JSON.parse(userStr);
      const userId = user.userId;
      
      if (!userId) {
        this.isBorrowing = false;
        this.translate.get('USER_ID_NOT_FOUND').subscribe((msg: string) => {
          this.toastService.error(msg);
        });
        return;
      }
      
      const borrowDto = {
        bookId: this.book!.bookId,
        userId: userId,
        borrowDate: new Date(),
        dueDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000)
      };
      
      // Borrow işlemini yap
      this.executeBorrow(borrowDto);
    } catch (error) {
      this.isBorrowing = false;
      this.translate.get('USER_INFO_LOAD_ERROR').subscribe((msg: string) => {
        this.toastService.error(msg);
      });
    }
  }

  // Borrow işlemini gerçekleştir
  private executeBorrow(borrowDto: any): void {
    if (!this.book) {
      console.error('Book bilgisi bulunamadı');
      this.isBorrowing = false;
      return;
    }

    const subscription = this.bookApi.borrow(this.book.bookId, borrowDto).subscribe({
      next: (response) => {
        this.isBorrowing = false;
        
        // State'i hemen güncelle
        if (this.book) {
          this.book.isAvailable = false;
        }
        if (this.bookStatus) {
          this.bookStatus.isBorrowedByUser = true;
          this.bookStatus.isAvailable = false;
        }
        
        // Backend'den güncel veriyi al
        this.loadBook(this.book!.bookId);
        this.loadBookStatus(this.book!.bookId);
        
        this.translate.get('BORROW_SUCCESS').subscribe((msg: string) => {
          this.toastService.success(msg);
        });
      },
      error: (err) => {
        this.isBorrowing = false;
        if (err.status === 401) {
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate(['/', lang, 'login']);
        } else if (err.status === 409) {
          // Kitap zaten ödünç alınmış
          this.translate.get('BOOK_ALREADY_BORROWED').subscribe((msg: string) => {
            this.toastService.warning(msg);
          });
          // Book status'u yenile
          this.loadBookStatus(this.book!.bookId);
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
    
    // LocalStorage'dan user bilgisini al
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      this.isBorrowing = false;
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }
    
    try {
      const user = JSON.parse(userStr);
      const userId = user.userId;
      
      if (!userId) {
        this.isBorrowing = false;
        this.translate.get('USER_ID_NOT_FOUND').subscribe((msg: string) => {
          this.toastService.error(msg);
        });
        return;
      }
      
      // Return işlemini yap
      this.executeReturn(userId);
    } catch (error) {
      this.isBorrowing = false;
      this.translate.get('USER_INFO_LOAD_ERROR').subscribe((msg: string) => {
        this.toastService.error(msg);
      });
    }
  }

  // Return işlemini gerçekleştir
  private executeReturn(userId: number): void {
    if (!this.book) {
      console.error('Book bilgisi bulunamadı');
      this.isBorrowing = false;
      return;
    }

    const subscription = this.bookApi.return(this.book.bookId, userId).subscribe({
      next: (response) => {
        this.isBorrowing = false;
        
        // State'i hemen güncelle
        if (this.book) {
          this.book.isAvailable = true;
        }
        if (this.bookStatus) {
          this.bookStatus.isBorrowedByUser = false;
          this.bookStatus.isAvailable = true;
        }
        
        // Backend'den güncel veriyi al
        this.loadBook(this.book!.bookId);
        this.loadBookStatus(this.book!.bookId);
        
        this.translate.get('RETURN_SUCCESS').subscribe((msg: string) => {
          this.toastService.success(msg);
        });
      },
      error: (err) => {
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
          this.isAddingToFavorites = false;
          this.handleError('ADD_FAVORITE_ERROR');
        }
      });
      this.subscriptions.push(subscription);
    }
  }

  checkIfUserLoggedIn(): boolean {
    if (!this.isBrowser) return false;
    return this.auth.isLoggedIn();
  }

  private getCurrentUserId(): number {
    // SSR sırasında localStorage mevcut değil
    if (!this.isBrowser) {
      return 1;
    }
    
    // Cookie tabanlı auth kullanıyoruz, localStorage'da user ID tutmuyoruz
    // Backend'den direkt user ID'yi al
    
    // Bu metod artık async olmalı, ama şimdilik borrow işlemini engelleyelim
    // User ID bulunamadığında borrow işlemini yapma
    return -1; // Geçersiz user ID
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