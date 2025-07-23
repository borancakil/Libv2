import {
  ApplicationConfig,
  importProvidersFrom,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { HttpClient, provideHttpClient, withFetch } from '@angular/common/http';

import { routes } from './app.routes';

// ngx-translate için importlar
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

// 💬 i18n çeviri dosyası yükleyici
export function HttpLoaderFactory(http: HttpClient) {
  // Projende i18n dosyaların neredeyse orayı göster. Genellikle ./assets/i18n/
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

export const appConfig: ApplicationConfig = {
  providers: [
    // <<< EKLENDİ: Angular'a Zone.js kullanmasını ve değişiklikleri otomatik algılamasını söylüyoruz.
    provideZoneChangeDetection({ eventCoalescing: true }),

    provideRouter(routes),
    provideHttpClient(withFetch()),

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
