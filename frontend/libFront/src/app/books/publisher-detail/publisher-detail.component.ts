import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Publisher } from '../models/publisher.model';
import { PublisherApiService } from '../services/publisher-api';

@Component({
  selector: 'app-publisher-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './publisher-detail.component.html',
  styleUrls: ['./publisher-detail.component.css'],
})
export class PublisherDetailComponent implements OnInit, OnDestroy {
  publisher: Publisher | null = null;
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    private publisherApi: PublisherApiService,
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
      const publisherId = parseInt(idParam, 10);
      
      if (!isNaN(publisherId)) {
        this.loadPublisher(publisherId);
      } else {
        this.handleError('PUBLISHER_DETAIL_INVALID_ID');
      }
    } else {
      this.handleError('PUBLISHER_DETAIL_NO_ID');
    }
  }

  private loadPublisher(id: number): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.publisherApi.getById(id).subscribe({
      next: (publisher) => {
        this.publisher = publisher;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading publisher:', err);
        this.isLoading = false;
        
        if (err.status === 404) {
          this.handleError('PUBLISHER_DETAIL_NOT_FOUND');
        } else {
          this.handleError('PUBLISHER_DETAIL_LOAD_ERROR');
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
    this.router.navigate(['/', lang, 'publishers']);
  }

  goToBookDetail(bookId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'book', bookId]);
  }

  retry(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const publisherId = parseInt(idParam, 10);
      if (!isNaN(publisherId)) {
        this.loadPublisher(publisherId);
      }
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 