import { Component, Inject, PLATFORM_ID, OnInit, OnDestroy, HostListener, ChangeDetectorRef } from '@angular/core';
import { NavigationEnd } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { filter } from 'rxjs/operators';
import { ToastComponent } from './components/toast/toast.component';
import { UserApiService } from './users/services/user-api';
import { AuthService } from './services/auth.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
  imports: [CommonModule, RouterModule, TranslateModule, ToastComponent],
})
export class AppComponent implements OnInit, OnDestroy {
  private isBrowser: boolean;
  private lastScrollTop = 0;
  private scrollThreshold = 10; // Minimum scroll mesafesi
  private scrollTimeout: any;

  // Header visibility state
  isHeaderVisible = true;
  isHeaderScrolling = false;

  constructor(
    public translate: TranslateService, 
    private router: Router,
    private userApi: UserApiService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    
    translate.addLangs(['en', 'tr']);
    translate.setDefaultLang('tr');

    // Dil dosyalarının yüklendiğini kontrol et
    translate.get('NAV_HOME').subscribe(
      (value) => {
        console.log('Dil dosyası yüklendi:', value);
      },
      (error) => {
        console.error('Dil dosyası yüklenemedi:', error);
      }
    );

    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => {
        const lang = this.getLangFromUrl();
        if (lang && translate.getLangs().includes(lang)) {
          this.translate.use(lang);
        }
        
        // Sayfa geçişlerinde en üste scroll
        if (this.isBrowser) {
          window.scrollTo(0, 0);
        }
      });
  }

  ngOnInit(): void {
    if (this.isBrowser) {
      this.lastScrollTop = window.pageYOffset || document.documentElement.scrollTop;
      // Listen to auth state changes to update header immediately
      window.addEventListener('authStateChanged', this.handleAuthStateChange);
      // Initialize auth state on first load if tokens exist
      this.updateAuthenticationState();
    }
  }

  ngOnDestroy(): void {
    if (this.scrollTimeout) {
      clearTimeout(this.scrollTimeout);
    }
    if (this.isBrowser) {
      window.removeEventListener('authStateChanged', this.handleAuthStateChange);
    }
  }

  @HostListener('window:scroll')
  onWindowScroll(): void {
    if (!this.isBrowser) return;

    const currentScrollTop = window.pageYOffset || document.documentElement.scrollTop;
    const scrollDifference = Math.abs(currentScrollTop - this.lastScrollTop);

    // Minimum scroll mesafesini geçtiyse işlem yap
    if (scrollDifference > this.scrollThreshold) {
      // Scroll timeout'u temizle
      if (this.scrollTimeout) {
        clearTimeout(this.scrollTimeout);
      }

      // Scroll yönünü belirle
      if (currentScrollTop > this.lastScrollTop && currentScrollTop > 100) {
        // Aşağı scroll - header'ı gizle
        this.isHeaderVisible = false;
        this.isHeaderScrolling = true;
      } else if (currentScrollTop < this.lastScrollTop) {
        // Yukarı scroll - header'ı göster
        this.isHeaderVisible = true;
        this.isHeaderScrolling = true;
      }

      this.lastScrollTop = currentScrollTop;

      // Scroll animasyonu bittikten sonra scrolling state'ini false yap
      this.scrollTimeout = setTimeout(() => {
        this.isHeaderScrolling = false;
      }, 300);
    }
  }

  getLangFromUrl(): string {
    const url = this.router.url;
    const parts = url.split('/');
    return parts[1]; // /tr/register → 'tr'
  }

  // Mobile menu state
  isMobileMenuOpen = false;
  isLanguageDropdownOpen = false;
  isUserMenuOpen = false;
  
  // Current language getter
  get currentLang(): string {
    return this.translate.currentLang || 'tr';
  }

  // Mobile menu methods
  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  // Language dropdown methods
  toggleLanguageDropdown(): void {
    this.isLanguageDropdownOpen = !this.isLanguageDropdownOpen;
  }

  // User menu methods
  toggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  closeUserMenu(): void {
    this.isUserMenuOpen = false;
  }

  switchLanguage(lang: string) {
    // Mevcut URL segmentlerini al
    const segments = this.router.url.split('/').filter(Boolean);

    if (segments.length > 0) {
      segments[0] = lang; // ilk segment = dil
    } else {
      segments.unshift(lang); // boşsa başa ekle
    }

    const newUrl = '/' + segments.join('/');
    this.isLanguageDropdownOpen = false;
    this.router.navigateByUrl(newUrl); // yönlendirme
  }

  isLoggedIn(): boolean {
    if (!this.isBrowser) return false;
    return this.auth.isLoggedIn();
  }

  // Authentication durumunu daha detaylı kontrol et
  isAuthenticated(): boolean {
    if (!this.isBrowser) {
      return false;
    }
    
    const isLoggedIn = this.isLoggedIn();
    const isAuthenticated = localStorage.getItem('isAuthenticated') === 'true';
    
    // Eğer localStorage'daki değer ile gerçek durum farklıysa güncelle
    if (isLoggedIn !== isAuthenticated) {
      this.updateAuthenticationState();
    }
    
    return isLoggedIn;
  }

  logout(): void {
    if (!this.isBrowser) return;
    this.userApi.logout().subscribe({
      next: () => {
        this.auth.clearTokens();
        const lang = this.translate.currentLang || 'tr';
        this.router.navigate([`/${lang}/login`]);
      },
      error: () => {
        this.auth.clearTokens();
        const lang = this.translate.currentLang || 'tr';
        this.router.navigate([`/${lang}/login`]);
      }
    });
  }

  // Cookie clearing not needed; tokens are in localStorage

  // Cookie-based session removed

  // Authentication state'ini güncelle
  private updateAuthenticationState(): void {
    if (!this.isBrowser) return;
    const isLoggedIn = this.isLoggedIn();
    localStorage.setItem('isAuthenticated', isLoggedIn.toString());
    if (typeof window !== 'undefined') {
      window.dispatchEvent(new CustomEvent('authStateChanged', { detail: { isLoggedIn } }));
    }
  }

  private handleAuthStateChange = (_event: any) => {
    // Ensure header re-renders when auth state changes
    this.cdr.detectChanges();
  }

  // User bilgilerini güncelle
  updateUserInfo(userInfo: any): void {
    if (!this.isBrowser) {
      return;
    }
    
    console.log('🔄 Updating user info in AppComponent:', userInfo);
    localStorage.setItem('user', JSON.stringify(userInfo));
    this.updateAuthenticationState();
  }

  // Tüm auth verilerini temizle
  clearAuthData(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem('user');
    localStorage.removeItem('isAuthenticated');
  }

  // Template için public metodlar
  getLocalStorageItem(key: string): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem(key);
  }

  // Kullanıcının gerçek adını al
  getUserName(): string {
    if (!this.isBrowser) {
      return '';
    }
    
    const user = localStorage.getItem('user');
    if (user) {
      try {
        const userObj = JSON.parse(user);
        return userObj.name || '';
      } catch (e) {
        console.error('Error parsing user data:', e);
        return '';
      }
    }
    return '';
  }

  // Kullanıcının gerçek email'ini al (User prefix'i olmadan)
  getUserEmail(): string {
    if (!this.isBrowser) {
      return '';
    }
    
    const user = localStorage.getItem('user');
    if (user) {
      try {
        const userObj = JSON.parse(user);
        return userObj.email || '';
      } catch (e) {
        console.error('Error parsing user data:', e);
        return '';
      }
    }
    return '';
  }

  // Kullanıcının tam adını al (isim soyisim)
  getUserFullName(): string {
    if (!this.isBrowser) {
      return '';
    }
    
    const user = localStorage.getItem('user');
    if (user) {
      try {
        const userObj = JSON.parse(user);
        const firstName = userObj.firstName || userObj.name || '';
        const lastName = userObj.lastName || '';
        
        if (firstName && lastName) {
          return `${firstName} ${lastName}`;
        } else if (firstName) {
          return firstName;
        } else {
          return '';
        }
      } catch (e) {
        console.error('Error parsing user data:', e);
        return '';
      }
    }
    return '';
  }

  getIsBrowser(): boolean {
    return this.isBrowser;
  }
}
