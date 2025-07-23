import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Author } from '../models/author.model';
import { AuthorApiService } from '../services/author-api';

@Component({
  selector: 'app-author-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './author-detail.component.html',
  styleUrls: ['./author-detail.component.css'],
})
export class AuthorDetailComponent implements OnInit, OnDestroy {
  author: Author | null = null;
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    private authorApi: AuthorApiService,
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
      const authorId = parseInt(idParam, 10);
      
      if (!isNaN(authorId)) {
        this.loadAuthor(authorId);
      } else {
        this.handleError('AUTHOR_DETAIL_INVALID_ID');
      }
    } else {
      this.handleError('AUTHOR_DETAIL_NO_ID');
    }
  }

  private loadAuthor(id: number): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.authorApi.getById(id).subscribe({
      next: (author) => {
        this.author = author;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading author:', err);
        this.isLoading = false;
        
        if (err.status === 404) {
          this.handleError('AUTHOR_DETAIL_NOT_FOUND');
        } else {
          this.handleError('AUTHOR_DETAIL_LOAD_ERROR');
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
    this.router.navigate(['/', lang, 'authors']);
  }

  goToBookDetail(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'book', bookId]);
  }

  retry(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const authorId = parseInt(idParam, 10);
      if (!isNaN(authorId)) {
        this.loadAuthor(authorId);
      }
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 