import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Publisher } from '../models/publisher.model';
import { Book } from '../models/book.model';

@Injectable({
  providedIn: 'root',
})
export class PublisherApiService {
  private apiUrl = 'https://localhost:7209/api';

  constructor(private http: HttpClient) {}

  getAll(filter?: string): Observable<Publisher[]> {
    let url = `${this.apiUrl}/Publishers`;
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
    return this.http.get<number>(
      `${this.apiUrl}/Publishers/${publisherId}/book-count`
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
} 