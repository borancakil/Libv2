import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Author } from '../models/author.model';
import { Book } from '../models/book.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthorApiService {
  private baseUrl = `${environment.apiUrl}/Authors`;

  constructor(private http: HttpClient) {}

  getAll(filter?: string): Observable<Author[]> {
    let url = `${this.baseUrl}/list`;
    if (filter && filter.trim()) {
      url += `?filter=${encodeURIComponent(filter.trim())}`;
    }
    return this.http.get<Author[]>(url);
  }

  getAllForList(filter?: string): Observable<Author[]> {
    let url = `${this.baseUrl}/list`;
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

  create(author: any): Observable<Author> {
    return this.http.post<Author>(this.baseUrl, author);
  }

  update(id: number, author: any): Observable<Author> {
    return this.http.put<Author>(`${this.baseUrl}/${id}`, author);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getAuthorBooks(authorId: number): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.baseUrl}/${authorId}/books`);
  }

  getBooksByAuthor(authorId: number): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.baseUrl}/${authorId}/books`);
  }

  getAuthorBookCount(authorId: number): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}/${authorId}/book-count`);
  }

  getAuthorProfileImage(authorId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${authorId}/profile-image`, { responseType: 'blob' });
  }

  deleteAuthorProfileImage(authorId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${authorId}/profile-image`);
  }

  authorExists(authorId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/${authorId}/exists`);
  }
} 