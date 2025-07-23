export interface Author {
  authorId: number;
  name: string;
  biography?: string;
  birthDate?: string;
  deathDate?: string;
  books?: Book[];
}

export interface CreateAuthorDto {
  name: string;
  biography?: string;
  birthDate?: string;
  deathDate?: string;
}

export interface UpdateAuthorDto {
  name?: string;
  biography?: string;
  birthDate?: string;
  deathDate?: string;
}

import { Book } from './book.model'; 