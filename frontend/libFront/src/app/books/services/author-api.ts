import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Author, CreateAuthorDto, UpdateAuthorDto } from '../models/author.model';

@Injectable({ providedIn: 'root' })
export class AuthorApiService {
  private baseUrl = 'http://localhost:5107/api/Authors';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Author[]> {
    return this.http.get<Author[]>(this.baseUrl);
  }

  getById(id: number): Observable<Author> {
    return this.http.get<Author>(`${this.baseUrl}/${id}`);
  }

  create(author: CreateAuthorDto): Observable<Author> {
    return this.http.post<Author>(this.baseUrl, author);
  }

  update(id: number, author: UpdateAuthorDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, author);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  exists(id: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/${id}/exists`);
  }
} 