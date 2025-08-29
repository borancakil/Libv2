import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { UserApiService } from '../services/user-api';
import { AuthService } from '../../services/auth.service';
import { User } from '../models/user.model';

interface EditForm {
  name: string;
  email: string;
  age?: number;
  gender?: string;
  address?: string;
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
    age: undefined,
    gender: '',
    address: ''
  };
  public isSaving: boolean = false;
  private isBrowser: boolean;

  constructor(
    private router: Router,
    private userApi: UserApiService,
    private translate: TranslateService,
    private auth: AuthService,
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
   * Load current user profile
   */
  loadUserProfile(): void {
    this.isLoading = true;
    this.error = null;

    this.userApi.getCurrentUserInfo().subscribe({
      next: (userData) => {
        this.user = userData;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading user profile:', err);
        this.isLoading = false;
        this.handleError('ERROR_LOADING_PROFILE');
      },
    });
  }

  /**
   * Start editing profile
   */
  startEditing(): void {
    if (!this.user) return;

    // Kullanıcıya onay sor
    const confirmed = confirm(this.translate.instant('CONFIRM_EDIT_PROFILE'));
    if (!confirmed) return;

    this.editForm = {
      name: this.user.name || '',
      email: this.user.email || '',
      age: this.user.age,
      gender: this.user.gender || '',
      address: this.user.address || ''
    };
    this.isEditing = true;
  }

  /**
   * Cancel editing
   */
  cancelEditing(): void {
    this.isEditing = false;
    this.editForm = {
      name: '',
      email: '',
      age: undefined,
      gender: '',
      address: ''
    };
  }

  /**
   * Save profile changes
   */
  saveProfile(): void {
    if (!this.user) return;

    this.isSaving = true;
    this.error = null;

    const updateData = {
      name: this.editForm.name,
      email: this.editForm.email,
      age: this.editForm.age,
      gender: this.editForm.gender,
      address: this.editForm.address
    };

    this.userApi.updateUser(this.user.userId, updateData).subscribe({
      next: () => {
        // Update local user data
        if (this.user) {
          this.user = {
            ...this.user,
            ...updateData
          };
        }
        
        this.isEditing = false;
        this.editForm = {
          name: '',
          email: '',
          age: undefined,
          gender: '',
          address: ''
        };
        this.isSaving = false;
        
        // Show success message
        this.showSuccessMessage('PROFILE_UPDATED_SUCCESS');
      },
      error: (err) => {
        console.error('Error updating profile:', err);
        this.isSaving = false;
        this.handleError('ERROR_UPDATING_PROFILE');
      },
    });
  }

  /**
   * Logout user
   */
  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  /**
   * Confirm logout
   */
  confirmLogout(): void {
    const confirmed = confirm(this.translate.instant('CONFIRM_LOGOUT'));
    if (confirmed) {
      this.logout();
    }
  }

  /**
   * Navigate back to dashboard
   */
  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  /**
   * Handle errors
   */
  private handleError(errorKey: string): void {
    this.error = this.translate.instant(errorKey);
  }

  /**
   * Show success message
   */
  private showSuccessMessage(messageKey: string): void {
    // You can implement a toast service here
    console.log(this.translate.instant(messageKey));
  }
} 