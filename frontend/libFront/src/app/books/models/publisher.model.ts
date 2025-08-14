import { Book } from './book.model';

export interface Publisher {
  publisherId: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  description?: string;
  foundedYear?: number;
  location?: string;
  books?: Book[];
  hasProfileImage?: boolean;
}

export interface CreatePublisherDto {
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  description?: string;
  foundedYear?: number;
  location?: string;
}

export interface UpdatePublisherDto {
  name?: string;
  address?: string;
  phone?: string;
  email?: string;
  description?: string;
  foundedYear?: number;
  location?: string;
} 