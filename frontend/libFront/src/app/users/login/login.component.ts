import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { UserApiService } from '../services/user-api';
import { AppComponent } from '../../app';

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
    private appComponent: AppComponent,
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

        // Session'ı ayarla
        if (this.isBrowser) {
          // Backend'den JWE token geliyor - decrypt etmeye çalışma
          // Token'ı olduğu gibi sakla, backend her API çağrısında decrypt edecek
          try {
            console.log('JWE Token received:', res.token);
            
            // JWE token'dan user bilgilerini çıkaramayız, backend'den almalıyız
            // Şimdilik basit user bilgisi kullan
            const userInfo = {
              userId: 1, // Backend'den alınacak
              email: credentials.email,
              name: 'User'
            };
            
            console.log('Using fallback user info for JWE token');
            
            // Session yönetimi ile kaydet (JWE token'ı olduğu gibi)
            this.appComponent.setSession(res.token, userInfo);
            console.log('Session set with JWE token');
            
            // TODO: Backend'den user bilgilerini al
            this.loadUserInfo(credentials.email);
            
          } catch (error) {
            console.error('Error handling JWE token:', error);
            // Fallback: basit user bilgisi
            const userInfo = {
              userId: 1,
              email: credentials.email,
              name: 'User'
            };
            this.appComponent.setSession(res.token, userInfo);
            console.log('Fallback session set due to JWE handling error');
          }
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

  // Backend'den user bilgilerini al
  private loadUserInfo(email: string): void {
    this.userApi.getCurrentUserInfo().subscribe({
      next: (userInfo) => {
        console.log('User info received from backend:', userInfo);
        // User bilgilerini güncelle
        this.appComponent.updateUserInfo(userInfo);
      },
      error: (err) => {
        console.error('Error loading user info:', err);
        // Fallback user info kullan
      }
    });
  }
}
