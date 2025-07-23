import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Publisher } from '../models/publisher.model';
import { PublisherApiService } from '../services/publisher-api';

@Component({
  selector: 'app-publisher-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './publisher-list.component.html',
  styleUrls: ['./publisher-list.component.css'],
})
export class PublisherListComponent implements OnInit, OnDestroy {
  publishers: Publisher[] = [];
  isLoading = true;
  error: string | null = null;
  private isBrowser: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
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

    this.loadPublishers();
  }

  private loadPublishers(): void {
    this.isLoading = true;
    this.error = null;

    const subscription = this.publisherApi.getAll().subscribe({
      next: (publishers) => {
        this.publishers = publishers;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading publishers:', err);
        this.isLoading = false;
        this.handleError('PUBLISHER_LIST_LOAD_ERROR');
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

  goToPublisherDetail(publisherId: number): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'publisher', publisherId]);
  }

  retry(): void {
    this.loadPublishers();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
} 