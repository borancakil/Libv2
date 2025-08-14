import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Book } from '../../books/models/book.model';
import { UserApiService } from '../services/user-api';

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.css'],
})
export class FavoritesComponent implements OnInit, OnDestroy {
  favoriteBooks: Book[] = [];
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

    this.loadFavoriteBooks();
  }

  private loadFavoriteBooks(): void {
    this.isLoading = true;
    this.error = null;

    console.log('Loading favorite books...'); // TEMPORARY DEBUG

    const subscription = this.userApi.getMyFavoriteBooks().subscribe({
      next: (books) => {
        console.log('Favorite books loaded:', books); // TEMPORARY DEBUG
        this.favoriteBooks = books;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading favorite books:', err);
        console.log('Error status:', err.status); // TEMPORARY DEBUG
        console.log('Error message:', err.message); // TEMPORARY DEBUG
        this.isLoading = false;
        
        if (err.status === 401) {
          // Token geçersiz, login sayfasına yönlendir
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate(['/', lang, 'login']);
        } else {
          this.handleError('FAVORITES_LOAD_ERROR');
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

  goToBookDetail(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'book', bookId]);
  }

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang]);
  }

  retry(): void {
    this.loadFavoriteBooks();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 