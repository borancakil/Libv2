import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';
import { Book } from '../../books/models/book.model';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserApiService {
  private baseUrl = `${environment.apiUrl}/Users`;

  constructor(private http: HttpClient) {}

  // Authentication
  login(credentials: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, credentials);
  }

  logout(): Observable<any> {
    return this.http.post(`${this.baseUrl}/logout`, {});
  }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, userData);
  }

  // Get current user info
  getCurrentUserInfo(): Observable<any> {
    return this.http.get(`${this.baseUrl}/current-user`);
  }

  // User management (Admin)
  getAllUsers(includeLoans: boolean = false): Observable<User[]> {
    return this.http.get<User[]>(
      `${this.baseUrl}?includeLoans=${includeLoans}`
    );
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/${id}`);
  }

  updateUser(id: number, user: any): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, user);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Role management (Admin)
  promoteToAdmin(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/promote`, {});
  }

  demoteToUser(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/demote`, {});
  }

  // Self-service (Authenticated user)
  updatePassword(id: number, passwordData: any): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/password`, passwordData);
  }

  getMyBorrowedBooks(): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.baseUrl}/my-borrowed-books`);
  }

  getMyFavoriteBooks(): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.baseUrl}/my-favorite-books`);
  }

  // Favori kontrolü - kullanıcının favori kitaplarını al ve kontrol et
  isBookInMyFavorites(bookId: number): Observable<boolean> {
    return this.http
      .get<Book[]>(`${this.baseUrl}/my-favorite-books`)
      .pipe(map((books) => books.some((book) => book.bookId === bookId)));
  }

  addToMyFavorites(bookId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/my-favorites/${bookId}`, {});
  }

  removeFromMyFavorites(bookId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/my-favorites/${bookId}`);
  }

  // Admin-only data access
  getBorrowedBooks(userId: number): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.baseUrl}/${userId}/borrowed-books`);
  }

  getFavoriteBooks(userId: number): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.baseUrl}/${userId}/favorite-books`);
  }

  addToFavorites(userId: number, bookId: number): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/${userId}/favorites/${bookId}`,
      {}
    );
  }

  removeFromFavorites(userId: number, bookId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/${userId}/favorites/${bookId}`
    );
  }

  // Existence checks
  userExists(id: number): Observable<boolean> {
    return this.http
      .head(`${this.baseUrl}/${id}`, { observe: 'response' })
      .pipe(
        map((response: any) => response.ok),
        catchError(() => of(false))
      );
  }

  emailExists(email: string): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(
      `${this.baseUrl}/email-exists?email=${encodeURIComponent(email)}`
    );
  }
}
