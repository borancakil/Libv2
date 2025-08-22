import {
  ApplicationConfig,
  importProvidersFrom,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { HttpClient, HttpRequest, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError, from, of, switchMap } from 'rxjs';
import { environment } from '../environments/environment';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';

import { routes } from './app.routes';

// ngx-translate için importlar
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

// 💬 i18n çeviri dosyası yükleyici
export function HttpLoaderFactory(http: HttpClient) {
  // Projende i18n dosyaların neredeyse orayı göster. Genellikle /assets/i18n/
  return new TranslateHttpLoader(http, '/assets/i18n/', '.json');
}

// Auth interceptor - cookie tabanlı authentication ve refresh token desteği
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  // SSR guard
  if (typeof window === 'undefined') {
    return next(req);
  }

  const accessToken = auth.getAccessToken();
  const accessExpiry = auth.getAccessExpiry();

  // If token missing
  if (!accessToken || !accessExpiry) {
    return next(req);
  }

  // If access token expired, try refresh then retry (Observable flow)
  if (Date.now() >= accessExpiry) {
    return from(refreshAccessToken(auth)).pipe(
      switchMap((ok) => {
        const latest = auth.getAccessToken();
        if (ok && latest) {
          const cloned = req.clone({
            setHeaders: { Authorization: `Bearer ${latest}` },
          });
          return next(cloned);
        }
        return next(req);
      })
    );
  }

  // Attach bearer
  const authReq = req.clone({
    setHeaders: { Authorization: `Bearer ${accessToken}` },
  });

  return next(authReq).pipe(
    catchError((error) => {
      if (error.status === 401) {
        return from(refreshAccessToken(auth)).pipe(
          switchMap((ok) => {
            const latest = auth.getAccessToken();
            if (ok && latest) {
              const retryReq = req.clone({
                setHeaders: { Authorization: `Bearer ${latest}` },
              });
              return next(retryReq);
            }
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};

// Refresh token ile yeni access token alma fonksiyonu
function refreshAccessToken(auth: AuthService): Promise<boolean> {
  if (typeof window === 'undefined') return Promise.resolve(false);
  const refreshToken = auth.getRefreshToken();
  if (!refreshToken) return Promise.resolve(false);
  const url = `${environment.apiUrl}/Users/refresh`;
  return fetch(url, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${refreshToken}`,
      'Content-Type': 'application/json'
    }
  }).then(async (resp) => {
    if (!resp.ok) return false;
    const data = await resp.json().catch(() => null) as any;
    if (data && data.accessToken && data.expiresIn) {
      const currentRefresh = auth.getRefreshToken() || refreshToken;
      auth.setTokens(data.accessToken, currentRefresh, data.expiresIn);
      return true;
    }
    return false;
  }).catch(() => false);
}

export const appConfig: ApplicationConfig = {
  providers: [
    // <<< EKLENDİ: Angular'a Zone.js kullanmasını ve değişiklikleri otomatik algılamasını söylüyoruz.
    provideZoneChangeDetection({ eventCoalescing: true }),

    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),

    // Animation provider'ı eklendi
    provideAnimations(),

    // ngx-translate provider'ları
    importProvidersFrom(
      TranslateModule.forRoot({
        defaultLanguage: 'en',
        loader: {
          provide: TranslateLoader,
          useFactory: HttpLoaderFactory,
          deps: [HttpClient],
        },
      })
    ),
  ],
};
