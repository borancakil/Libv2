import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Author } from '../models/author.model';
import { AuthorApiService } from '../services/author-api';

@Component({
  selector: 'app-author-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './author-list.component.html',
  styleUrls: ['./author-list.component.css'],
})
export class AuthorListComponent implements OnInit, OnDestroy {
  authors: Author[] = [];
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
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

    this.loadAuthors();
  }

  private loadAuthors(): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.authorApi.getAll().subscribe({
      next: (authors) => {
        this.authors = authors;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading authors:', err);
        this.isLoading = false;
        this.handleError('AUTHOR_LIST_LOAD_ERROR');
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

  goToAuthorDetail(authorId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'author', authorId]);
  }

  retry(): void {
    this.loadAuthors();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 