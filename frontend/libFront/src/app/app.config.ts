import {
  ApplicationConfig,
  importProvidersFrom,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { HttpClient, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

import { routes } from './app.routes';

// ngx-translate için importlar
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

// 💬 i18n çeviri dosyası yükleyici
export function HttpLoaderFactory(http: HttpClient) {
  // Projende i18n dosyaların neredeyse orayı göster. Genellikle /assets/i18n/
  return new TranslateHttpLoader(http, '/assets/i18n/', '.json');
}

// Auth interceptor - token'ı otomatik olarak ekler
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // SSR sırasında localStorage mevcut değil, kontrol et
  if (typeof window === 'undefined' || !window.localStorage) {
    return next(req);
  }
  
  const token = localStorage.getItem('authToken');
  
  if (token) {
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(authReq).pipe(
      catchError((error) => {
        if (error.status === 401) {
          localStorage.removeItem('authToken');
          localStorage.removeItem('user');
          // Sayfayı yenile
          window.location.reload();
        }
        return throwError(() => error);
      })
    );
  }
  
  return next(req);
};

export const appConfig: ApplicationConfig = {
  providers: [
    // <<< EKLENDİ: Angular'a Zone.js kullanmasını ve değişiklikleri otomatik algılamasını söylüyoruz.
    provideZoneChangeDetection({ eventCoalescing: true }),

    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),

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
