// START OF FILE register.component.ts

import { Component, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { UserApiService } from '../services/user-api';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnDestroy {
  firstName = '';
  lastName = '';
  password = '';
  confirmPassword = '';
  email = '';
  showPassword = false;
  showConfirmPassword = false;
  isLoading = false;
  error: string | null = null;
  successMessage: string | null = null;
  private isBrowser: boolean;

  get currentLang(): string {
    return this.translate.currentLang || 'tr';
  }

  private apiSub: Subscription | undefined;

  constructor(
    private userApi: UserApiService,
    public translate: TranslateService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  getPasswordStrength(): string {
    if (!this.password) return '';
    
    const hasLetter = /[a-zA-Z]/.test(this.password);
    const hasNumber = /\d/.test(this.password);
    const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(this.password);
    const length = this.password.length;
    
    if (length < 6) return 'weak';
    if (length >= 8 && hasLetter && hasNumber && hasSpecial) return 'strong';
    if (length >= 6 && hasLetter && hasNumber) return 'medium';
    return 'weak';
  }

  getPasswordStrengthText(): string {
    const strength = this.getPasswordStrength();
    switch (strength) {
      case 'weak': return 'Zayıf şifre';
      case 'medium': return 'Orta güçlükte şifre';
      case 'strong': return 'Güçlü şifre';
      default: return '';
    }
  }

  register(): void {
    if (!this.isBrowser) {
      return; // SSR sırasında API çağrısı yapma
    }

    this.error = null;
    this.successMessage = null;
    this.isLoading = true;

    const user = {
      name: `${this.firstName} ${this.lastName}`,
      password: this.password,
      email: this.email,
    };

    console.log('Attempting registration with:', user);

    this.apiSub = this.userApi.register(user).subscribe({
      next: (res) => {
        console.log('Registration response received:', res);
        this.isLoading = false;

        this.translate
          .get('REGISTER_SUCCESS')
          .subscribe((msg: string) => {
            this.successMessage = msg;
          });

        setTimeout(() => {
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate([`/${lang}/login`]);
        }, 2000);
      },
      error: (err) => {
        console.error('Registration failed with error:', err);
        console.error('Error status:', err.status);
        console.error('Error message:', err.message);
        console.error('Error response:', err.error);
        this.isLoading = false;
        
        // Detaylı hata mesajları
        if (err.status === 400) {
          if (err.error?.errors) {
            // Validation errors
            const errorObj = err.error.errors;
            this.error = Object.values(errorObj)
              .flat()
              .map((error) => String(error))
              .join(', ');
          } else if (err.error?.message) {
            // Backend'den gelen hata mesajı
            this.error = err.error.message;
          } else {
            // Genel 400 hatası
            this.translate
              .get('REGISTER_ERROR_BAD_REQUEST')
              .subscribe((msg: string) => {
                this.error = msg;
              });
          }
                 } else if (err.status === 409) {
           // Conflict - Email zaten kayıtlı
           this.translate
             .get('REGISTER_ERROR_EMAIL_EXISTS')
             .subscribe((msg: string) => {
               this.error = msg;
             });
         } else if (err.status === 422) {
           // Unprocessable Entity - Validation hatası
           if (err.error?.message) {
             this.error = err.error.message;
           } else {
             this.translate
               .get('REGISTER_ERROR_VALIDATION')
               .subscribe((msg: string) => {
                 this.error = msg;
               });
           }
         } else {
           // Genel hata
           this.translate
             .get('REGISTER_ERROR_GENERAL')
             .subscribe((msg: string) => {
               this.error = msg;
             });
         }
      },
    });
  }

  ngOnDestroy(): void {
    this.apiSub?.unsubscribe();
  }
}
// END OF FILE register.component.ts
