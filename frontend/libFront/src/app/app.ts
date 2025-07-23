import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { NavigationEnd } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
  imports: [CommonModule, RouterModule, TranslateModule],
})
export class AppComponent {
  private isBrowser: boolean;

  constructor(
    public translate: TranslateService, 
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    
    translate.addLangs(['en', 'tr']);
    translate.setDefaultLang('tr');

    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => {
        const lang = this.getLangFromUrl();
        if (lang && translate.getLangs().includes(lang)) {
          this.translate.use(lang);
        }
      });
  }

  getLangFromUrl(): string {
    const url = this.router.url;
    const parts = url.split('/');
    return parts[1]; // /tr/register → 'tr'
  }

  // Mobile menu state
  isMobileMenuOpen = false;
  isLanguageDropdownOpen = false;
  
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
    if (!this.isBrowser) {
      return false; // SSR sırasında false döndür
    }
    return !!sessionStorage.getItem('auth_token');
  }

  logout(): void {
    if (!this.isBrowser) {
      return; // SSR sırasında bir şey yapma
    }
    sessionStorage.removeItem('auth_token');
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate([`/${lang}/login`]);
  }
}
