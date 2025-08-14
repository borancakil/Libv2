export interface Author {
  authorId: number;
  name: string;
  biography?: string;
  birthDate?: string;
  deathDate?: string;
  books?: Book[];
  hasProfileImage?: boolean;
}

export interface CreateAuthorDto {
  name: string;
  biography?: string;
  birthDate?: string;
  deathDate?: string;
  profileImage?: File;
}

export interface UpdateAuthorDto {
  name?: string;
  biography?: string;
  birthDate?: string;
  deathDate?: string;
  profileImage?: File;
}

import { Book } from './book.model'; 