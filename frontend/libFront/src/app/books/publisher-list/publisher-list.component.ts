import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Publisher } from '../models/publisher.model';
import { PublisherApiService } from '../services/publisher-api';

@Component({
  selector: 'app-publisher-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, FormsModule],
  templateUrl: './publisher-list.component.html',
  styleUrls: ['./publisher-list.component.css'],
})
export class PublisherListComponent implements OnInit, OnDestroy {
  filter: string = '';
  publishers: Publisher[] = [];
  filteredPublishers: Publisher[] = [];
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];
  private isInitialized = false;

  // Search and filter properties
  searchTerm = '';
  selectedFilter = 'all';
  selectedSort = 'name';
  searchTimeout: any;

  // Filter properties
  filters: any = {
    name: '',
    description: '',
    foundedYear: undefined,
    location: '',
    sortBy: 'name'
  };
  
  // Filter expansion state
  isFilterExpanded = false;

  constructor(
    public router: Router,
    private publisherApi: PublisherApiService,
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
    this.loadPublishers();
  }

  private loadPublishers(): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.publisherApi.getAll(this.filter).subscribe({
      next: (publishers) => {
        this.publishers = publishers;
        this.filteredPublishers = [...publishers];
        this.isLoading = false;
        this.applyFilters();
      },
      error: (err) => {
        console.error('Error loading publishers:', err);
        this.isLoading = false;
        this.handleError('PUBLISHER_LIST_LOAD_ERROR');
      },
    });

    this.subscriptions.push(subscription);
  }

  onFilterButtonClick(): void {
    const newFilter = (this.filter || '').trim();
    
    if (newFilter !== this.filter) {
      this.filter = newFilter;
      this.loadPublishers();
    } else {
      this.applyLocalFilters();
    }
  }

  getTotalBooks(): number {
    // Return 0 since we don't show book counts in list
    return 0;
  }

  getPublisherBookCount(publisherId: number): number {
    // Return 0 since we don't show book counts in list
    return 0;
  }

  // Search and filter methods
  onSearchInput(): void {
    clearTimeout(this.searchTimeout);
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
      description: '',
      foundedYear: undefined,
      location: '',
      sortBy: 'name'
    };
    this.applyLocalFilters();
  }

  hasActiveFilters(): boolean {
    return !!(this.filters.name?.trim() || 
              this.filters.description?.trim() || 
              this.filters.foundedYear ||
              this.filters.location?.trim());
  }

  private applyFilters(): void {
    let filtered = [...this.publishers];

    if (this.searchTerm.trim() !== '') {
      const searchLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(publisher => 
        publisher.name.toLowerCase().includes(searchLower) ||
        (publisher.description && publisher.description.toLowerCase().includes(searchLower))
      );
    }

    filtered.sort((a, b) => {
      switch (this.selectedSort) {
        case 'name':
          return a.name.localeCompare(b.name);
        default:
          return 0;
      }
    });

    this.filteredPublishers = filtered;
  }

  private applyLocalFilters(): void {
    let filtered = [...this.publishers];

    if (this.filters.name?.trim()) {
      const nameLower = this.filters.name.toLowerCase();
      filtered = filtered.filter(publisher => 
        publisher.name.toLowerCase().includes(nameLower)
      );
    }

    if (this.filters.description?.trim()) {
      const descLower = this.filters.description.toLowerCase();
      filtered = filtered.filter(publisher => 
        publisher.description && publisher.description.toLowerCase().includes(descLower)
      );
    }

    if (this.filters.foundedYear) {
      filtered = filtered.filter(publisher => 
        publisher.foundedYear && publisher.foundedYear === this.filters.foundedYear
      );
    }

    if (this.filters.location?.trim()) {
      const locationLower = this.filters.location.toLowerCase();
      filtered = filtered.filter(publisher => 
        publisher.location && publisher.location.toLowerCase().includes(locationLower)
      );
    }

    if (this.filters.sortBy) {
      filtered.sort((a, b) => {
        switch (this.filters.sortBy) {
          case 'name':
            return a.name.localeCompare(b.name);
          case 'foundedYear':
            if (!a.foundedYear && !b.foundedYear) return 0;
            if (!a.foundedYear) return 1;
            if (!b.foundedYear) return -1;
            return a.foundedYear - b.foundedYear;
          default:
            return 0;
        }
      });
    }

    this.filteredPublishers = filtered;
  }

  goToPublisherDetail(publisherId: number): void {
    this.router.navigate(['/tr/publisher', publisherId]);
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  retry(): void {
    if (!this.isLoading) {
      this.loadPublishers();
    }
  }

  private handleError(errorKey: string): void {
    this.translate.get(errorKey).subscribe((message) => {
      this.error = message;
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 