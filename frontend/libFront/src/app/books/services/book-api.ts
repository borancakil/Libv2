import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Book, BookListDto, CreateBookDto, UpdateBookDto, BorrowBookDto, BookStatusForUser, FavoriteStatus, FavoriteCount, BookFilterDto } from '../models/book.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BookApiService {
  private baseUrl = `${environment.apiUrl}/Books`;

  constructor(private http: HttpClient) {}

  getAll(
    filter: string = '',
    includeUnavailable: boolean = true
  ): Observable<BookListDto[]> {
    const filterParam = filter
      ? `&filter=${encodeURIComponent(filter.trim())}`
      : '';
    return this.http.get<BookListDto[]>(
      `${this.baseUrl}/list?includeUnavailable=${includeUnavailable}${filterParam}`
    );
  }

  // New filter method using backend API
  filterBooks(filter: BookFilterDto): Observable<Book[]> {
    return this.http.post<Book[]>(`${this.baseUrl}/filter`, filter);
  }

  getById(id: number): Observable<Book> {
    return this.http.get<Book>(`${this.baseUrl}/${id}`);
  }

  create(book: CreateBookDto): Observable<any> {
    const formData = new FormData();
    formData.append('title', book.title);
    formData.append('publicationYear', book.publicationYear.toString());
    formData.append('authorId', book.authorId.toString());
    formData.append('publisherId', book.publisherId.toString());

    if (book.coverImage) {
      formData.append('coverImage', book.coverImage);
    }

    return this.http.post<any>(this.baseUrl, formData);
  }

  update(id: number, book: UpdateBookDto): Observable<void> {
    const formData = new FormData();

    if (book.title) formData.append('title', book.title);
    if (book.publicationYear)
      formData.append('publicationYear', book.publicationYear.toString());
    if (book.authorId) formData.append('authorId', book.authorId.toString());
    if (book.publisherId)
      formData.append('publisherId', book.publisherId.toString());
    if (book.coverImage) formData.append('coverImage', book.coverImage);

    return this.http.put<void>(`${this.baseUrl}/${id}`, formData);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Image handling
  getCoverImage(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/cover`, {
      responseType: 'blob',
    });
  }

  removeCoverImage(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}/cover`);
  }

  // Book borrowing and returning
  borrow(id: number, borrowDto: BorrowBookDto): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${id}/borrow`, borrowDto);
  }

  return(id: number, userId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${id}/return`, { userId });
  }

  // Book status and borrowing checks
  getBookStatusForUser(bookId: number): Observable<BookStatusForUser> {
    return this.http.get<BookStatusForUser>(
      `${this.baseUrl}/${bookId}/status-for-user`
    );
  }



  isBorrowedByUser(bookId: number, userId: number): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/${bookId}/borrowed-by/${userId}`
    );
  }

  // Favorites functionality
  getFavoriteCount(bookId: number): Observable<FavoriteCount> {
    return this.http.get<FavoriteCount>(
      `${this.baseUrl}/${bookId}/favorite-count`
    );
  }

  isFavoritedByUser(
    bookId: number,
    userId: number
  ): Observable<FavoriteStatus> {
    return this.http.get<FavoriteStatus>(
      `${this.baseUrl}/${bookId}/favorited-by/${userId}`
    );
  }

  // Existence checks
  exists(id: number): Observable<boolean> {
    return this.http
      .head(`${this.baseUrl}/${id}`, { observe: 'response' })
      .pipe(
        map((response: any) => response.ok),
        catchError((err: any) => {
          if (err.status === 404) {
            return of(false);
          }
          return throwError(() => err);
        })
      );
  }

  authorExists(id: number): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(
      `${this.baseUrl}/authors/${id}/exists`
    );
  }

  publisherExists(id: number): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(
      `${this.baseUrl}/publishers/${id}/exists`
    );
  }
}
