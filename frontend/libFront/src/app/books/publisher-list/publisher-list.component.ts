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
        this.filteredPublishers = publishers;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading publishers:', err);
        this.isLoading = false;
        this.handleError('PUBLISHERS_LOAD_ERROR');
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
      this.filteredPublishers = [...this.publishers];
      return;
    }

    const term = filterTerm.toLowerCase();
    
    this.filteredPublishers = this.publishers.filter(publisher => {
      // Name filter
      if (publisher.name && publisher.name.toLowerCase().includes(term)) {
        return true;
      }
      
      // Address filter
      if (publisher.address && publisher.address.toLowerCase().includes(term)) {
        return true;
      }
      
      // Contact email filter
      if (publisher.contactEmail && publisher.contactEmail.toLowerCase().includes(term)) {
        return true;
      }
      
      // Established year filter
      if (publisher.establishedDate) {
        const establishedYear = new Date(publisher.establishedDate).getFullYear().toString();
        if (establishedYear.includes(term)) {
          return true;
        }
      }
      
      return false;
    });
  }

  clearFilters(): void {
    this.filter = '';
    this.filteredPublishers = [...this.publishers];
  }

  goToPublisherDetail(publisherId: number): void {
    this.router.navigate(['/publishers', publisherId]);
  }

  goBack(): void {
    this.router.navigate(['/books']);
  }

  retry(): void {
    this.loadPublishers();
  }

  private handleError(errorKey: string): void {
    this.error = this.translate.instant(errorKey);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 