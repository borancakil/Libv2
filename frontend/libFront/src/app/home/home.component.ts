import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BookApiService } from '../books/services/book-api';
import { Book, BookListDto } from '../books/models/book.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    FormsModule,
    HttpClientModule,
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  public searchQuery: string = '';
  public popularBooks: BookListDto[] = [];
  public totalBooks: number = 0;
  public isLoading: boolean = false;
  public error: string | null = null;
  private isBrowser: boolean;

  constructor(
    private router: Router,
    private bookApiService: BookApiService,
    private translate: TranslateService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (this.isBrowser) {
      this.loadPopularBooks();
    }
  }

  /**
   * Kullanıcıyı arama sonuçları sayfasına yönlendirir.
   */
  search(): void {
    if (this.searchQuery.trim()) {
      // Arama terimi ile birlikte /search rotasına yönlendir
      this.router.navigate(['/search'], {
        queryParams: { q: this.searchQuery.trim() },
      });
    }
  }

  /**
   * Backend API'den popüler kitapları yükler
   */
  public loadPopularBooks(): void {
    if (!this.isBrowser) {
      return; // SSR sırasında API çağrısı yapma
    }

    this.isLoading = true;
    this.error = null;

    this.bookApiService.getAll("", true).subscribe({
      next: (books) => {
        // Toplam kitap sayısını kaydet
        this.totalBooks = books.length;
        // İlk 6 kitabı alıyoruz (popüler kitaplar olarak gösterelim)
        this.popularBooks = books.slice(0, 6);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Kitaplar yüklenirken hata oluştu:', error);
        this.error = 'Kitaplar yüklenirken bir hata oluştu.';
        this.isLoading = false;
        this.popularBooks = [];
      },
    });
  }

  /**
   * Kitap detay sayfasına yönlendir
   */
  viewBookDetails(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigateByUrl(`/${lang}/book/${bookId}`);
  }

  /**
   * Kitaplar sayfasına yönlendir
   */
  goToBooks(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'books']);
  }

  /**
   * Yazarlar sayfasına yönlendir
   */
  goToAuthors(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'authors']);
  }

  /**
   * Yayınevleri sayfasına yönlendir
   */
  goToPublishers(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'publishers']);
  }

  /**
   * Kitap kapağı URL'sini alır
   */
  getBookCoverUrl(bookId: number): string {
    return `http://localhost:7208/api/Books/${bookId}/cover`;
  }

  /**
   * Resim yüklenemediğinde placeholder gösterir
   */
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


}
