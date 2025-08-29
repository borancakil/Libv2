import { Book } from './book.model';

export interface Author {
  authorId: number;
  name: string;
  biography?: string;
  nationality?: string;
  birthDate?: string;
  bookCount?: number;
  profileImageUrl?: string;
  books?: Book[];
}

export interface CreateAuthorDto {
  name: string;
  biography?: string;
  nationality?: string;
  birthDate?: string;
}

export interface UpdateAuthorDto {
  name: string;
  biography?: string;
  nationality?: string;
  birthDate?: string;
} 