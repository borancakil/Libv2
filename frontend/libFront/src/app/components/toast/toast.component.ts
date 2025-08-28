import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { ToastService, Toast } from '../../services/toast.service';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  animations: [
    trigger('toastAnimation', [
      state('void', style({
        opacity: 0,
        transform: 'translateY(-100%)'
      })),
      transition(':enter', [
        animate('300ms ease-out', style({
          opacity: 1,
          transform: 'translateY(0)'
        }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({
          opacity: 0,
          transform: 'translateY(-100%)'
        }))
      ])
    ])
  ],
  template: `
    <div class="toast-container">
      <div 
        *ngFor="let toast of toasts; trackBy: trackByToast"
        class="toast"
        [class]="'toast-' + toast.type"
        [@toastAnimation]="toast.id"
        (click)="removeToast(toast.id)"
      >
        <div class="toast-content">
          <div class="toast-icon" *ngIf="toast.icon">
            <i [class]="toast.icon"></i>
          </div>
          <div class="toast-message">
            <span>{{ toast.message }}</span>
          </div>
          <button 
            class="toast-close" 
            (click)="removeToast(toast.id); $event.stopPropagation()"
            aria-label="Close notification"
          >
            <i class="fas fa-times"></i>
          </button>
        </div>
        
        <div class="toast-progress" *ngIf="toast.duration">
          <div class="progress-bar" [style.animation-duration]="toast.duration + 'ms'"></div>
        </div>

        <div class="toast-action" *ngIf="toast.action">
          <button 
            class="action-btn"
            (click)="executeAction(toast.action!.callback); $event.stopPropagation()"
          >
            {{ toast.action!.label }}
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./toast.component.css']
})
export class ToastComponent implements OnInit, OnDestroy {
  toasts: Toast[] = [];
  private subscription: Subscription = new Subscription();

  constructor(private toastService: ToastService) {}

  ngOnInit(): void {
    this.subscription = this.toastService.getToasts().subscribe(toasts => {
      this.toasts = toasts;
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  removeToast(id: string): void {
    this.toastService.remove(id);
  }

  executeAction(callback: () => void): void {
    callback();
  }

  trackByToast(index: number, toast: Toast): string {
    return toast.id;
  }
} 