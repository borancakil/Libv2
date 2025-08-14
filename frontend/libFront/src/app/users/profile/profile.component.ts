import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { UserApiService } from '../services/user-api';
import { User } from '../models/user.model';

interface EditForm {
  name: string;
  email: string;
  phone: string;
  address: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    FormsModule,
    HttpClientModule,
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css'],
})
export class ProfileComponent implements OnInit {
  public user: User | null = null;
  public isLoading: boolean = false;
  public error: string | null = null;
  public isEditing: boolean = false;
  public editForm: EditForm = {
    name: '',
    email: '',
    phone: '',
    address: ''
  };
  public isSaving: boolean = false;
  private isBrowser: boolean;

  constructor(
    private router: Router,
    private userApi: UserApiService,
    private translate: TranslateService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (this.isBrowser) {
      this.loadUserProfile();
    }
  }

  /**
   * Load user profile data
   */
  loadUserProfile(): void {
    if (!this.isBrowser) {
      return;
    }

    this.isLoading = true;
    this.error = null;

    // Get user data from localStorage for now
    const userData = localStorage.getItem('user');
    if (userData) {
      try {
        this.user = JSON.parse(userData);
        this.isLoading = false;
      } catch (error) {
        console.error('Error parsing user data:', error);
        this.error = 'Error loading user profile';
        this.isLoading = false;
      }
    } else {
      this.error = 'User not found';
      this.isLoading = false;
    }
  }

  /**
   * Start editing profile
   */
  startEditing(): void {
    if (this.user) {
      this.editForm = {
        name: this.user.name || '',
        email: this.user.email || '',
        phone: this.user.phone || '',
        address: this.user.address || ''
      };
      this.isEditing = true;
    }
  }

  /**
   * Cancel editing
   */
  cancelEditing(): void {
    this.isEditing = false;
    this.editForm = {
      name: '',
      email: '',
      phone: '',
      address: ''
    };
  }

  /**
   * Save profile changes
   */
  saveProfile(): void {
    if (!this.user) return;

    this.isSaving = true;
    
    // Simulate API call - replace with actual API call
    setTimeout(() => {
      if (this.user) {
        this.user = {
          ...this.user,
          ...this.editForm
        };
        
        // Update localStorage
        localStorage.setItem('user', JSON.stringify(this.user));
        
        this.isEditing = false;
        this.editForm = {
          name: '',
          email: '',
          phone: '',
          address: ''
        };
        this.isSaving = false;
      }
    }, 1000);
  }

  /**
   * Navigate back to dashboard
   */
  goBack(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'dashboard']);
  }

  /**
   * Navigate to dashboard
   */
  goToDashboard(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'dashboard']);
  }

  /**
   * Navigate to favorites
   */
  goToFavorites(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'favorites']);
  }

  /**
   * Navigate to borrowed books
   */
  goToBorrowedBooks(): void {
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang, 'borrowed']);
  }

  /**
   * Logout user
   */
  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    localStorage.removeItem('sessionExpiry');
    
    const lang = this.translate.currentLang || 'tr';
    this.router.navigate(['/', lang]);
  }

  /**
   * Check if user is logged in
   */
  checkIfUserLoggedIn(): boolean {
    const authToken = localStorage.getItem('authToken');
    const user = localStorage.getItem('user');
    return !!(authToken && user);
  }

  /**
   * Get current user ID
   */
  getCurrentUserId(): number | null {
    const userData = localStorage.getItem('user');
    if (userData) {
      try {
        const user = JSON.parse(userData);
        return user.id || null;
      } catch {
        return null;
      }
    }
    return null;
  }
} 