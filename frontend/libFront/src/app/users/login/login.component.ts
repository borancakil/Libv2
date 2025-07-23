import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { UserApiService } from '../services/user-api';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  email = '';
  password = '';
  showPassword = false;
  isLoading = false;
  error: string | null = null;
  successMessage: string | null = null;
  
  private isBrowser: boolean;

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

  login(): void {
    if (!this.isBrowser) {
      return; // SSR sırasında API çağrısı yapma
    }

    this.isLoading = true;
    this.error = null;
    this.successMessage = null;

    const credentials = {
      email: this.email,
      password: this.password,
    };

    this.userApi.login(credentials).subscribe({
      next: (res) => {
        console.log('Login successful', res);
        this.isLoading = false;

        // Token'ı sessionStorage'a kaydet
        if (this.isBrowser) {
          sessionStorage.setItem('auth_token', res.token);
        }

        // Başarı mesajı göster
        this.translate.get('LOGIN_SUCCESS').subscribe((msg: string) => {
          this.successMessage = msg;
        });

        // 1 saniye sonra yönlendir
        setTimeout(() => {
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate([`/${lang}`]);
        }, 1000);
      },
      error: (err) => {
        console.error('Login failed', err);
        this.isLoading = false;
        
        // Hata mesajını çeviri ile göster
        if (err.status === 401) {
          this.translate.get('LOGIN_ERROR_INVALID').subscribe((msg: string) => {
            this.error = msg;
          });
        } else {
          this.translate.get('LOGIN_ERROR_GENERAL').subscribe((msg: string) => {
            this.error = msg;
          });
        }
      },
    });
  }
}
