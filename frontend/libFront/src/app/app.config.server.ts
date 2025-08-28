import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering } from '@angular/platform-server';
import { appConfig } from './app.config'; // Ortak config'i import et

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(),
  ],
};

// Ortak config ile sunucu config'ini birleştir
export const config = mergeApplicationConfig(appConfig, serverConfig);
