import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Author } from '../models/author.model';
import { AuthorApiService } from '../services/author-api';

@Component({
  selector: 'app-author-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, FormsModule],
  templateUrl: './author-list.component.html',
  styleUrls: ['./author-list.component.css'],
})
export class AuthorListComponent implements OnInit, OnDestroy {
  filter: string = '';
  authors: Author[] = [];
  filteredAuthors: Author[] = [];
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
      return;
    }
    this.loadAuthors();
  }

  private loadAuthors(): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.authorApi.getAll(this.filter).subscribe({
      next: (authors) => {
        this.authors = authors;
        this.filteredAuthors = authors;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading authors:', err);
        this.isLoading = false;
        this.handleError('AUTHORS_LOAD_ERROR');
      },
    });

    this.subscriptions.push(subscription);
  }

  onFilterInput(): void {
    // Anlık filtreleme kaldırıldı - sadece Enter tuşuna basıldığında filtreleme yapılacak
  }

  onFilterButtonClick(): void {
    this.applyFilters();
  }

  private applyFilters(): void {
    const filterTerm = (this.filter || '').trim();
    
    if (!filterTerm) {
      this.filteredAuthors = [...this.authors];
      return;
    }

    const term = filterTerm.toLowerCase();
    
    this.filteredAuthors = this.authors.filter(author => {
      // Name filter
      if (author.name && author.name.toLowerCase().includes(term)) {
        return true;
      }
      
      // Biography filter
      if (author.biography && author.biography.toLowerCase().includes(term)) {
        return true;
      }
      
      // Nationality filter
      if (author.nationality && author.nationality.toLowerCase().includes(term)) {
        return true;
      }
      
      // Birth year filter
      if (author.birthDate) {
        const birthYear = new Date(author.birthDate).getFullYear().toString();
        if (birthYear.includes(term)) {
          return true;
        }
      }
      
      return false;
    });
  }

  clearFilters(): void {
    this.filter = '';
    this.filteredAuthors = [...this.authors];
  }

  goToAuthorDetail(authorId: number): void {
    this.router.navigate(['/authors', authorId]);
  }

  goBack(): void {
    this.router.navigate(['/books']);
  }

  retry(): void {
    this.loadAuthors();
  }

  private handleError(errorKey: string): void {
    this.error = this.translate.instant(errorKey);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 