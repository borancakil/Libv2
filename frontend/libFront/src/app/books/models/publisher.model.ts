import { Book } from './book.model';

export interface Publisher {
  publisherId: number;
  name: string;
  address?: string;
  contactEmail?: string;
  establishedDate?: string;
  bookCount?: number;
  logoImageUrl?: string;
  books?: Book[];
}

export interface CreatePublisherDto {
  name: string;
  address?: string;
  contactEmail?: string;
  establishedDate?: string;
}

export interface UpdatePublisherDto {
  name: string;
  address?: string;
  contactEmail?: string;
  establishedDate?: string;
} 