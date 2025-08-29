import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  setTokens(accessToken: string, refreshToken: string, expiresInSeconds: number): void {
    if (!this.isBrowser) return;
    const accessExpiry = Date.now() + expiresInSeconds * 1000;
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    localStorage.setItem('accessExpiry', accessExpiry.toString());
    localStorage.setItem('isAuthenticated', 'true');
    this.emitAuthState(true);
  }

  clearTokens(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('accessExpiry');
    localStorage.removeItem('isAuthenticated');
    localStorage.removeItem('user');
    this.emitAuthState(false);
  }

  getAccessToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('refreshToken');
  }

  getAccessExpiry(): number | null {
    if (!this.isBrowser) return null;
    const raw = localStorage.getItem('accessExpiry');
    if (!raw) return null;
    const n = parseInt(raw, 10);
    return Number.isFinite(n) ? n : null;
  }

  isLoggedIn(): boolean {
    if (!this.isBrowser) return false;
    const token = this.getAccessToken();
    const expiry = this.getAccessExpiry();
    if (!token || !expiry) return false;
    return Date.now() < expiry;
  }

  logout(): void {
    this.clearTokens();
  }

  private emitAuthState(isLoggedIn: boolean): void {
    if (typeof window === 'undefined') return;
    window.dispatchEvent(new CustomEvent('authStateChanged', { detail: { isLoggedIn } }));
  }
}


