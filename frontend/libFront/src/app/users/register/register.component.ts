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
  name = '';
  password = '';
  email = '';
  showPassword = false;
  isLoading = false;

  public validationErrors: string[] = [];
  public successMessage: string | null = null;
  private isBrowser: boolean;

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

  register(): void {
    if (!this.isBrowser) {
      return; // SSR sırasında API çağrısı yapma
    }

    this.validationErrors = [];
    this.successMessage = null;
    this.isLoading = true;

    const user = {
      name: this.name,
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
            this.validationErrors = Object.values(errorObj)
              .flat()
              .map((error) => String(error));
          } else if (err.error?.message) {
            // Backend'den gelen hata mesajı
            this.validationErrors = [err.error.message];
          } else {
            // Genel 400 hatası
            this.translate
              .get('REGISTER_ERROR_BAD_REQUEST')
              .subscribe((msg: string) => {
                this.validationErrors = [msg];
              });
          }
        } else if (err.status === 409) {
          // Conflict - Email zaten kayıtlı
          this.translate
            .get('REGISTER_ERROR_EMAIL_EXISTS')
            .subscribe((msg: string) => {
              this.validationErrors = [msg];
            });
        } else if (err.status === 422) {
          // Unprocessable Entity - Validation hatası
          if (err.error?.message) {
            this.validationErrors = [err.error.message];
          } else {
            this.translate
              .get('REGISTER_ERROR_VALIDATION')
              .subscribe((msg: string) => {
                this.validationErrors = [msg];
              });
          }
        } else {
          // Genel hata
          this.translate
            .get('REGISTER_ERROR_GENERAL')
            .subscribe((msg: string) => {
              this.validationErrors = [msg];
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
