import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Publisher, CreatePublisherDto, UpdatePublisherDto } from '../models/publisher.model';

@Injectable({ providedIn: 'root' })
export class PublisherApiService {
  private baseUrl = 'http://localhost:5107/api/Publishers';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Publisher[]> {
    return this.http.get<Publisher[]>(this.baseUrl);
  }

  getById(id: number): Observable<Publisher> {
    return this.http.get<Publisher>(`${this.baseUrl}/${id}`);
  }

  create(publisher: CreatePublisherDto): Observable<Publisher> {
    return this.http.post<Publisher>(this.baseUrl, publisher);
  }

  update(id: number, publisher: UpdatePublisherDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, publisher);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  exists(id: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/${id}/exists`);
  }
} 