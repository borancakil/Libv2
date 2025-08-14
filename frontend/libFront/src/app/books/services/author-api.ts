import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Author, CreateAuthorDto, UpdateAuthorDto } from '../models/author.model';
import { Book } from '../models/book.model';

@Injectable({ providedIn: 'root' })
export class AuthorApiService {
  private baseUrl = 'https://localhost:7209/api/Authors';

  constructor(private http: HttpClient) {}

  getAll(filter?: string): Observable<Author[]> {
    let url = this.baseUrl;
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
    return this.http.get<number>(`${this.baseUrl}/${authorId}/book-count`);
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
} 