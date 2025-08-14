export enum BookCategory {
  Fiction = 1,
  NonFiction = 2,
  Science = 3,
  History = 4,
  Philosophy = 5,
  Technology = 6,
  Art = 7,
  Literature = 8,
  Biography = 9,
  Children = 10
}

export interface Book {
  bookId: number;
  title: string;
  publicationYear: number;
  isAvailable: boolean;
  authorId: number;
  authorName?: string;
  publisherId: number;
  publisherName?: string;
  categories?: BookCategory[];
  borrowCount?: number;
  currentBorrower?: any;
  author?: Author;
  publisher?: Publisher;
  hasCoverImage?: boolean;
  returnDate?: string;
  remainingDays?: number;
  isOverdue?: boolean;
}

export interface Author {
  authorId: number;
  name: string;
  biography?: string;
  birthDate?: string;
  deathDate?: string;
  books?: Book[];
}

export interface Publisher {
  publisherId: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  books?: Book[];
}

export interface CreateBookDto {
  title: string;
  publicationYear: number;
  authorId: number;
  publisherId: number;
  categories?: BookCategory[];
  coverImage?: File;
}

export interface UpdateBookDto {
  title?: string;
  publicationYear?: number;
  authorId?: number;
  publisherId?: number;
  categories?: BookCategory[];
  coverImage?: File;
}

export interface BookFilterDto {
  title?: string;
  author?: string;
  publisher?: string;
  year?: number;
  isAvailable?: boolean;
  categories?: BookCategory[];
  minRating?: number;
  maxRating?: number;
}

export interface BorrowBookDto {
  bookId: number;
  userId: number;
  borrowDate: string;
  returnDate: string;
}

export interface BookStatusForUser {
  bookId: number;
  userId: number;
  isAvailable: boolean;
  isBorrowedByUser: boolean;
  canBorrow: boolean;
  canReturn: boolean;
  action: 'return' | 'borrow' | 'unavailable';
  returnDate?: string;
  remainingDays?: number;
  isOverdue?: boolean;
}

export interface FavoriteStatus {
  isFavorited: boolean;
  bookId: number;
  userId: number;
}

export interface FavoriteCount {
  bookId: number;
  favoriteCount: number;
}
