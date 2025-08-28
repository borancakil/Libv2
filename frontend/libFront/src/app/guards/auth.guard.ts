import { Injectable, inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const isLoggedIn = auth.isLoggedIn();
  if (!isLoggedIn) {
    // Redirect to current language login
    const lang = (window as any)?.appLanguage || 'tr';
    router.navigate(['/', lang, 'login']);
    return false;
  }
  return true;
};


