import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Book } from '../models/book.model';
import { BookApiService } from '../services/book-api';

@Component({
  selector: 'app-book-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './book-detail.component.html',
  styleUrls: ['./book-detail.component.css'],
})
export class BookDetailComponent implements OnInit, OnDestroy {
  book: Book | null = null;
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    private bookApi: BookApiService,
    public translate: TranslateService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser) {
      return; // SSR sırasında API çağrısı yapma
    }

    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (idParam) {
      const bookId = parseInt(idParam, 10);
      
      if (!isNaN(bookId)) {
        this.loadBook(bookId);
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

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message: string) => {
      this.error = message;
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
    // Kullanıcı giriş yapmış mı kontrol et
    const isLoggedIn = this.checkIfUserLoggedIn();
    
    if (!isLoggedIn) {
      // Giriş yapmamışsa login sayfasına yönlendir
      const lang = this.translate.currentLang || 'tr';
      this.router.navigate(['/', lang, 'login']);
      return;
    }

    if (!this.book || !this.book.isAvailable) {
      console.log('Kitap mevcut değil veya ödünç alınamaz');
      return;
    }

    console.log('Ödünç alma işlemi başlatılıyor...');
    console.log('Kullanıcı giriş durumu:', isLoggedIn);
    console.log('Kitap durumu:', this.book);

    // Ödünç alma işlemi için gerekli bilgileri topla
    const borrowDto = {
      bookId: this.book.bookId,
      userId: this.getCurrentUserId(), // Kullanıcı ID'sini al
      borrowDate: new Date().toISOString(),
      returnDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString() // 14 gün sonra
    };

    console.log('Borrow DTO:', borrowDto);

    this.bookApi.borrow(this.book.bookId, borrowDto).subscribe({
      next: (response) => {
        console.log('Kitap başarıyla ödünç alındı:', response);
        // Kitabı yeniden yükle
        this.loadBook(this.book!.bookId);
      },
      error: (err) => {
        console.error('Ödünç alma hatası:', err);
        // Hata mesajını göster
        this.handleError('BORROW_ERROR');
      }
    });
  }

  private checkIfUserLoggedIn(): boolean {
    // Local storage'dan token kontrolü
    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('user');
    
    // Token ve user bilgisi varsa giriş yapmış sayılır
    return !!(token && user);
  }

  private getCurrentUserId(): number {
    // Local storage'dan user ID'yi al
    const userStr = localStorage.getItem('user');
    if (userStr) {
      const user = JSON.parse(userStr);
      return user.userId || 1; // Varsayılan olarak 1
    }
    return 1; // Varsayılan kullanıcı ID
  }

  retry(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const bookId = parseInt(idParam, 10);
      if (!isNaN(bookId)) {
        this.loadBook(bookId);
      }
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 