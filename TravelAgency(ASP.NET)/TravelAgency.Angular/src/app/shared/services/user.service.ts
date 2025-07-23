import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@environments/environment';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';

import { LoginDto } from '../models/dtos/loginDto';
import { UserDto } from '../models/dtos/userDto';
import { LocalStorageService } from './localstorage.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = environment.apiUrl + "/users";
  private loggedInSubject = new Subject<boolean>();
  loggedInStatusChanged$ = this.loggedInSubject.asObservable();


  constructor(private http: HttpClient, private localStorage: LocalStorageService) { }

  updateLoggedInStatus(status: boolean) {
    if(!status){
      this.localStorage.removeItem('user');
    }
    this.loggedInSubject.next(status);
  }

  createUser(userDto: UserDto): Observable<UserDto> {
    return this.http.post<UserDto>(this.apiUrl, userDto);
  }

  login(loginDto: LoginDto): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/login`, loginDto).pipe(
      tap(userDto => {
        this.localStorage.setItem('user', JSON.stringify(userDto));
        this.updateLoggedInStatus(true);
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/logout`, {}).pipe(
      tap(_ => {
        this.updateLoggedInStatus(false);
      })
    );
  }

  isLoggedIn(): boolean {
    const user = this.localStorage.getItem('user');
    return user != null;
  }

  getUser(): UserDto | null {
    return this.localStorage.getItem('user');
  }
}
