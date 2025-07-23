export interface Book {
  bookId: number;
  title: string;
  publicationYear: number;
  isAvailable: boolean;
  authorId: number;
  authorName?: string;
  publisherId: number;
  publisherName?: string;
  borrowCount?: number;
  currentBorrower?: any;
  author?: Author;
  publisher?: Publisher;
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
}

export interface UpdateBookDto {
  title?: string;
  publicationYear?: number;
  authorId?: number;
  publisherId?: number;
}

export interface BorrowBookDto {
  bookId: number;
  userId: number;
  borrowDate: string;
  returnDate: string;
}
