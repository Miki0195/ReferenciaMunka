import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@environments/environment';
import { Observable } from 'rxjs';

import { RentDto } from '../models/dtos/rentDto';

@Injectable({
  providedIn: 'root'
})
export class RentService {
  private apiUrl = environment.apiUrl + "/rents";

  constructor(private http: HttpClient) { }

  createRent(rentDto: RentDto): Observable<RentDto> {
    return this.http.post<RentDto>(`${this.apiUrl}`, rentDto);
  }

  getRents(apartmentId: number): Observable<RentDto[]> {
    let params = new HttpParams();
    params = params.set('apartmentId', apartmentId);

    return this.http.get<RentDto[]>(`${this.apiUrl}`, { params: params } );
  }
}
