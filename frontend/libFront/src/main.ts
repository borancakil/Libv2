import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app'; // App component'in adını doğru yazdığından emin ol
import { appConfig } from './app/app.config';

bootstrapApplication(AppComponent, appConfig) // Sadece appConfig'i kullan
  .catch((err) => console.error(err));
