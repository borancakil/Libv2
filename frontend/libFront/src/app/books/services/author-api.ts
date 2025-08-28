import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Author, CreateAuthorDto, UpdateAuthorDto } from '../models/author.model';
import { Book } from '../models/book.model';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthorApiService {
  private baseUrl = `${environment.apiUrl}/Authors`;
  private bookCountCache = new Map<number, number>(); // Simple cache for book counts

  constructor(private http: HttpClient) {}

  getAll(filter?: string): Observable<Author[]> {
    let url = this.baseUrl;
    if (filter && filter.trim()) {
      url += `?filter=${encodeURIComponent(filter.trim())}`;
    }
    return this.http.get<Author[]>(url);
  }

  getAllWithDetails(filter?: string): Observable<Author[]> {
    let url = `${this.baseUrl}/details`;
    if (filter && filter.trim()) {
      url += `?filter=${encodeURIComponent(filter.trim())}`;
    }
    return this.http.get<Author[]>(url);
  }

  getById(id: number): Observable<Author> {
    return this.http.get<Author>(`${this.baseUrl}/${id}`);
  }

  getBooksByAuthor(authorId: number): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.baseUrl}/${authorId}/books`);
  }

  getBookCount(authorId: number): Observable<number> {
    // Check cache first
    if (this.bookCountCache.has(authorId)) {
      return of(this.bookCountCache.get(authorId)!);
    }

    return this.http.get<any>(`${this.baseUrl}/${authorId}/book-count`).pipe(
      map(response => {
        let count = 0;
        // Handle {"count":5} format
        if (response && typeof response === 'object' && 'count' in response) {
          count = response.count || 0;
        }
        // Handle direct number response
        else if (typeof response === 'number') {
          count = response;
        }
        // Handle other object formats
        else if (response && typeof response === 'object') {
          count = response.count || response.bookCount || response.totalBooks || response.value || 0;
        }
        
        // Cache the result
        this.bookCountCache.set(authorId, count);
        return count;
      })
    );
  }

  getProfileImage(authorId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${authorId}/profile-image`, { responseType: 'blob' });
  }

  create(author: CreateAuthorDto): Observable<Author> {
    return this.http.post<Author>(this.baseUrl, author);
  }

  update(id: number, author: UpdateAuthorDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, author);
  }

  removeProfileImage(authorId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${authorId}/profile-image`);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  exists(id: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/${id}/exists`);
  }

  // Method to clear cache when needed
  clearBookCountCache(): void {
    this.bookCountCache.clear();
  }

  // Method to clear specific author from cache
  clearAuthorFromCache(authorId: number): void {
    this.bookCountCache.delete(authorId);
  }
} 