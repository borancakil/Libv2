import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Toast {
  id: string;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  icon?: string;
  duration?: number;
  action?: {
    label: string;
    callback: () => void;
  };
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toasts = new BehaviorSubject<Toast[]>([]);

  constructor() {}

  getToasts(): Observable<Toast[]> {
    return this.toasts.asObservable();
  }

  show(toast: Omit<Toast, 'id'>): void {
    const newToast: Toast = {
      ...toast,
      id: this.generateId(),
      duration: toast.duration || 4000
    };

    const currentToasts = this.toasts.value;
    this.toasts.next([...currentToasts, newToast]);

    // Auto remove after duration
    setTimeout(() => {
      this.remove(newToast.id);
    }, newToast.duration);
  }

  success(message: string, options?: Partial<Toast>): void {
    this.show({
      message,
      type: 'success',
      icon: 'fas fa-check-circle',
      ...options
    });
  }

  error(message: string, options?: Partial<Toast>): void {
    this.show({
      message,
      type: 'error',
      icon: 'fas fa-exclamation-circle',
      ...options
    });
  }

  warning(message: string, options?: Partial<Toast>): void {
    this.show({
      message,
      type: 'warning',
      icon: 'fas fa-exclamation-triangle',
      ...options
    });
  }

  info(message: string, options?: Partial<Toast>): void {
    this.show({
      message,
      type: 'info',
      icon: 'fas fa-info-circle',
      ...options
    });
  }

  remove(id: string): void {
    const currentToasts = this.toasts.value;
    this.toasts.next(currentToasts.filter(toast => toast.id !== id));
  }

  clear(): void {
    this.toasts.next([]);
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }
} 