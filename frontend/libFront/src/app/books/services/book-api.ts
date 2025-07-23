import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Book, CreateBookDto, UpdateBookDto, BorrowBookDto } from '../models/book.model';

@Injectable({ providedIn: 'root' })
export class BookApiService {
  private baseUrl = 'http://localhost:5107/api/Books';

  constructor(private http: HttpClient) {}

  getAll(includeUnavailable: boolean = true): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.baseUrl}?includeUnavailable=${includeUnavailable}`);
  }

  getById(id: number): Observable<Book> {
    return this.http.get<Book>(`${this.baseUrl}/${id}`);
  }

  create(book: CreateBookDto): Observable<any> {
    return this.http.post<any>(this.baseUrl, book);
  }

  update(id: number, book: UpdateBookDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, book);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  borrow(id: number, borrowDto: BorrowBookDto): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${id}/borrow`, borrowDto);
  }

  return(id: number, userId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${id}/return`, userId);
  }

  exists(id: number): Observable<boolean> {
    return this.http.head(`${this.baseUrl}/${id}`, { observe: 'response' })
      .pipe(
        map((response: any) => response.ok),
        catchError((err: any) => {
          if (err.status === 404) {
            return of(false); // kitap bulunamadı
          }
          return throwError(() => err); // başka bir hata
        })
      );
  }
}
