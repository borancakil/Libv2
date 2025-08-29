import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Publisher } from '../models/publisher.model';
import { Book } from '../models/book.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class PublisherApiService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAll(filter?: string): Observable<Publisher[]> {
    let url = `${this.apiUrl}/Publishers/list`;
    if (filter && filter.trim()) {
      url += `?filter=${encodeURIComponent(filter.trim())}`;
    }
    return this.http.get<Publisher[]>(url);
  }

  getAllForList(filter?: string): Observable<Publisher[]> {
    let url = `${this.apiUrl}/Publishers/list`;
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

  create(publisher: any): Observable<Publisher> {
    return this.http.post<Publisher>(`${this.apiUrl}/Publishers`, publisher);
  }

  update(id: number, publisher: any): Observable<Publisher> {
    return this.http.put<Publisher>(`${this.apiUrl}/Publishers/${id}`, publisher);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Publishers/${id}`);
  }

  getBooksByPublisher(publisherId: number): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.apiUrl}/Publishers/${publisherId}/books`);
  }

  getPublisherBooks(publisherId: number): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.apiUrl}/Publishers/${publisherId}/books`);
  }

  getPublisherBookCount(publisherId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/Publishers/${publisherId}/book-count`);
  }

  getPublisherLogoImage(publisherId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/Publishers/${publisherId}/logo-image`, { responseType: 'blob' });
  }

  deletePublisherLogoImage(publisherId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Publishers/${publisherId}/logo-image`);
  }

  publisherExists(publisherId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/Publishers/${publisherId}/exists`);
  }
} 