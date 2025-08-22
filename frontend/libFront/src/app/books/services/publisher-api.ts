import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { Publisher } from '../models/publisher.model';
import { Book } from '../models/book.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class PublisherApiService {
  private apiUrl = environment.apiUrl;
  private bookCountCache = new Map<number, number>(); // Simple cache for book counts

  constructor(private http: HttpClient) {}

  getAll(filter?: string): Observable<Publisher[]> {
    let url = `${this.apiUrl}/Publishers`;
    if (filter && filter.trim()) {
      url += `?filter=${encodeURIComponent(filter.trim())}`;
    }
    return this.http.get<Publisher[]>(url);
  }

  getAllWithDetails(filter?: string): Observable<Publisher[]> {
    let url = `${this.apiUrl}/Publishers/details`;
    if (filter && filter.trim()) {
      url += `?filter=${encodeURIComponent(filter.trim())}`;
    }
    return this.http.get<Publisher[]>(url);
  }

  getById(id: number): Observable<Publisher> {
    return this.http.get<Publisher>(`${this.apiUrl}/Publishers/${id}`);
  }

  getBooksByPublisher(publisherId: number): Observable<Book[]> {
    return this.http.get<Book[]>(
      `${this.apiUrl}/Publishers/${publisherId}/books`
    );
  }

  getBookCount(publisherId: number): Observable<number> {
    // Check cache first
    if (this.bookCountCache.has(publisherId)) {
      return of(this.bookCountCache.get(publisherId)!);
    }

    return this.http.get<any>(
      `${this.apiUrl}/Publishers/${publisherId}/book-count`
    ).pipe(
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
        this.bookCountCache.set(publisherId, count);
        return count;
      })
    );
  }

  getProfileImage(publisherId: number): Observable<Blob> {
    return this.http.get(
      `${this.apiUrl}/Publishers/${publisherId}/profile-image`,
      { responseType: 'blob' }
    );
  }

  removeProfileImage(publisherId: number): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/Publishers/${publisherId}/profile-image`
    );
  }

  // Basic search endpoint - if it exists
  searchByName(name: string): Observable<Publisher[]> {
    return this.http.get<Publisher[]>(
      `${this.apiUrl}/Publishers/search?name=${encodeURIComponent(name)}`
    );
  }

  // Method to clear cache when needed
  clearBookCountCache(): void {
    this.bookCountCache.clear();
  }

  // Method to clear specific publisher from cache
  clearPublisherFromCache(publisherId: number): void {
    this.bookCountCache.delete(publisherId);
  }
} 