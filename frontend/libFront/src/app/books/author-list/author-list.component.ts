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
  private isInitialized = false;
  
  // Search properties
  searchTerm = '';
  selectedFilter = 'all';
  private searchTimeout: any;

  // Filter properties
  filters: any = {
    name: '',
    biography: '',
    status: '',
    birthYear: undefined,
    deathYear: undefined,
    sortBy: 'name'
  };
  
  // Filter expansion state
  isFilterExpanded = false;

  constructor(
    public router: Router,
    private authorApi: AuthorApiService,
    public translate: TranslateService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser || this.isInitialized) {
      return;
    }

    this.isInitialized = true;
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

  onFilterButtonClick(): void {
    const newFilter = (this.filter || '').trim();
    
    if (newFilter !== this.filter) {
      this.filter = newFilter;
      this.loadAuthors();
    } else {
      this.applyLocalFilters();
    }
  }

  // Search methods
  onSearchInput(): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    
    this.searchTimeout = setTimeout(() => {
      this.applyFilters();
    }, 300);
  }

  performSearch(): void {
    this.applyFilters();
  }

  // Filter expansion methods
  expandFilters(): void {
    this.isFilterExpanded = true;
  }

  collapseFilters(): void {
    this.isFilterExpanded = false;
  }

  // New backend filtering methods
  onFilterChange(): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    
    this.searchTimeout = setTimeout(() => {
      this.applyLocalFilters();
    }, 500);
  }

  applyBackendFilters(): void {
    this.applyLocalFilters();
  }

  clearAllFilters(): void {
    this.filters = {
      name: '',
      biography: '',
      status: '',
      birthYear: undefined,
      deathYear: undefined,
      sortBy: 'name'
    };
    this.applyLocalFilters();
  }

  hasActiveFilters(): boolean {
    return !!(this.filters.name?.trim() || 
              this.filters.biography?.trim() || 
              this.filters.status || 
              this.filters.birthYear ||
              this.filters.deathYear);
  }

  private applyFilters(): void {
    let filtered = [...this.authors];

    if (this.searchTerm.trim() !== '') {
      const searchLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(author => 
        author.name.toLowerCase().includes(searchLower) ||
        (author.biography && author.biography.toLowerCase().includes(searchLower))
      );
    }

    if (this.selectedFilter !== 'all') {
      filtered = filtered.filter(author => {
        if (this.selectedFilter === 'alive') {
          return !author.deathDate;
        } else if (this.selectedFilter === 'deceased') {
          return !!author.deathDate;
        }
        return true;
      });
    }

    this.filteredAuthors = filtered;
  }

  private applyLocalFilters(): void {
    let filtered = [...this.authors];

    if (this.filters.name?.trim()) {
      const nameLower = this.filters.name.toLowerCase();
      filtered = filtered.filter(author => 
        author.name.toLowerCase().includes(nameLower)
      );
    }

    if (this.filters.biography?.trim()) {
      const bioLower = this.filters.biography.toLowerCase();
      filtered = filtered.filter(author => 
        author.biography && author.biography.toLowerCase().includes(bioLower)
      );
    }

    if (this.filters.status) {
      filtered = filtered.filter(author => {
        if (this.filters.status === 'alive') {
          return !author.deathDate;
        } else if (this.filters.status === 'deceased') {
          return !!author.deathDate;
        }
        return true;
      });
    }

    if (this.filters.birthYear) {
      filtered = filtered.filter(author => 
        author.birthDate && new Date(author.birthDate).getFullYear() === this.filters.birthYear
      );
    }

    if (this.filters.deathYear) {
      filtered = filtered.filter(author => 
        author.deathDate && new Date(author.deathDate).getFullYear() === this.filters.deathYear
      );
    }

    if (this.filters.sortBy) {
      filtered.sort((a, b) => {
        switch (this.filters.sortBy) {
          case 'name':
            return a.name.localeCompare(b.name);
          case 'birthDate':
            if (!a.birthDate && !b.birthDate) return 0;
            if (!a.birthDate) return 1;
            if (!b.birthDate) return -1;
            return new Date(a.birthDate).getTime() - new Date(b.birthDate).getTime();
          default:
            return 0;
        }
      });
    }

    this.filteredAuthors = filtered;
  }

  getTotalBooksCount(): number {
    // Return 0 since we don't show book counts in list
    return 0;
  }

  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang]);
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
    if (!this.isLoading) {
      this.loadAuthors();
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 