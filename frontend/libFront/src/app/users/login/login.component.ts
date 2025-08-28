import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { UserApiService } from '../services/user-api';
import { AppComponent } from '../../app';
// CookieUtilityService removed; tokens now stored in localStorage
import { AuthService } from '../../services/auth.service';

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

  get currentLang(): string {
    return this.translate.currentLang || 'tr';
  }

  constructor(
    private userApi: UserApiService,
    public translate: TranslateService,
    private router: Router,
    private appComponent: AppComponent,
    private auth: AuthService,
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

    console.log('Attempting login with:', credentials);

    this.userApi.login(credentials).subscribe({
      next: (res) => {
        console.log('Login response received:', res);
        this.isLoading = false;

        // Token-based auth - store tokens in localStorage
        if (this.isBrowser && res && res.accessToken && res.refreshToken && res.expiresIn) {
          this.auth.setTokens(res.accessToken, res.refreshToken, res.expiresIn);
          this.loadUserInfo();
        }

        // Başarı mesajı göster
        this.translate.get('LOGIN_SUCCESS').subscribe((msg: string) => {
          this.successMessage = msg;
        });

        // User bilgileri yüklendikten sonra yönlendir
        setTimeout(() => {
          const lang = this.translate.currentLang || 'tr';
          console.log('🔄 Redirecting to:', `/${lang}`);
          this.router.navigate([`/${lang}`]);
        }, 1000); // Daha hızlı yönlendirme
      },
      error: (err) => {
        console.error('Login failed with error:', err);
        console.error('Error status:', err.status);
        console.error('Error message:', err.message);
        console.error('Error response:', err.error);
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

  // Cookie kontrolü kaldırıldı - direkt user info yükle
  private waitForCookieAndLoadUser(): void {
    console.log('🔄 Loading user info directly (cookie check removed)...');
    this.loadUserInfo();
  }

  // Backend'den user bilgilerini al
  private loadUserInfo(): void {
    console.log('Loading user info from backend...');
    this.userApi.getCurrentUserInfo().subscribe({
      next: (userInfo) => {
        console.log('✅ User info received from backend:', userInfo);
        // User bilgilerini güncelle
        this.appComponent.updateUserInfo(userInfo);
        console.log('✅ User info updated in AppComponent');
      },
      error: (err) => {
        console.error('❌ Error loading user info:', err);
        console.error('Error details:', err.status, err.message);
        
        // 401 hatası varsa kısa bir bekleme sonra tekrar dene
        if (err.status === 401) {
          console.log('🔄 Unauthorized error, retrying after 500ms...');
          setTimeout(() => {
            this.loadUserInfo();
          }, 500);
        }
      }
    });
  }
}
