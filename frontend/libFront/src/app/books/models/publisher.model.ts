export interface Publisher {
  publisherId: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  books?: Book[];
}

export interface CreatePublisherDto {
  name: string;
  address?: string;
  phone?: string;
  email?: string;
}

export interface UpdatePublisherDto {
  name?: string;
  address?: string;
  phone?: string;
  email?: string;
}

import { Book } from './book.model'; 