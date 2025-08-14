import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { BookListComponent } from './books/book-list/book-list.component';
import { BookDetailComponent } from './books/book-detail/book-detail.component';
import { AuthorListComponent } from './books/author-list/author-list.component';
import { AuthorDetailComponent } from './books/author-detail/author-detail.component';
import { PublisherListComponent } from './books/publisher-list/publisher-list.component';
import { PublisherDetailComponent } from './books/publisher-detail/publisher-detail.component';
import { LoginComponent } from './users/login/login.component';
import { RegisterComponent } from './users/register/register.component';
import { BorrowedBooksComponent } from './users/borrowed-books/borrowed-books.component';
import { FavoritesComponent } from './users/favorites/favorites.component';
import { UserDashboardComponent } from './users/user-dashboard/user-dashboard.component';
import { ProfileComponent } from './users/profile/profile.component';

export const routes: Routes = [
  {
    path: ':lang',
    children: [
      { path: '', component: HomeComponent },
      { path: 'books', component: BookListComponent },
      { path: 'book/:id', component: BookDetailComponent },
      { path: 'authors', component: AuthorListComponent },
      { path: 'author/:id', component: AuthorDetailComponent },
      { path: 'publishers', component: PublisherListComponent },
      { path: 'publisher/:id', component: PublisherDetailComponent },
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'borrowed', component: BorrowedBooksComponent },
      { path: 'favorites', component: FavoritesComponent },
      { path: 'dashboard', component: UserDashboardComponent },
      { path: 'profile', component: ProfileComponent },
    ],
  },
  { path: '', redirectTo: '/tr', pathMatch: 'full' },
];
