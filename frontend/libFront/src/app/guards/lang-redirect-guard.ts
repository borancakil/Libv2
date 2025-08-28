// src/app/guards/lang-redirect.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class LangRedirectGuard implements CanActivate {
  constructor(private router: Router, private translate: TranslateService) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const selectedLang = this.translate.currentLang || 'tr';
    const originalPath = route.url.map((segment) => segment.path).join('/');
    const newUrl = `/${selectedLang}/${originalPath}`;
    this.router.navigateByUrl(newUrl);
    return false;
  }
}
     