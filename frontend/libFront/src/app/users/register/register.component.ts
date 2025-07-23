// START OF FILE register.component.ts

import { Component, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { UserApiService } from '../services/user-api';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnDestroy {
  name = '';
  password = '';
  email = '';
  showPassword = false;
  isLoading = false;

  public validationErrors: string[] = [];
  public successMessage: string | null = null;
  private isBrowser: boolean;

  private apiSub: Subscription | undefined;

  constructor(
    private userApi: UserApiService,
    public translate: TranslateService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  register(): void {
    if (!this.isBrowser) {
      return; // SSR sırasında API çağrısı yapma
    }

    this.validationErrors = [];
    this.successMessage = null;
    this.isLoading = true;

    const user = {
      name: this.name,
      password: this.password,
      email: this.email,
    };

    this.apiSub = this.userApi.register(user).subscribe({
      next: (res) => {
        console.log('Registration successful', res);
        this.isLoading = false;

        this.translate
          .get('REGISTER_SUCCESS')
          .subscribe((msg: string) => {
            this.successMessage = msg;
          });

        setTimeout(() => {
          const lang = this.translate.currentLang || 'tr';
          this.router.navigate([`/${lang}/login`]);
        }, 2000);
      },
      error: (err) => {
        console.error('Registration failed', err);
        this.isLoading = false;
        
        if (err.status === 400 && err.error?.errors) {
          const errorObj = err.error.errors;
          this.validationErrors = Object.values(errorObj)
            .flat()
            .map((error) => String(error));
        } else {
          this.translate
            .get('REGISTER_ERROR_GENERAL')
            .subscribe((msg: string) => {
              this.validationErrors = [msg];
            });
        }
      },
    });
  }

  ngOnDestroy(): void {
    this.apiSub?.unsubscribe();
  }
}
// END OF FILE register.component.ts
