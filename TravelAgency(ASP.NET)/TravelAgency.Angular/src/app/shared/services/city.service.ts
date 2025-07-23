import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@environments/environment';
import { Observable } from 'rxjs';

import { CityDto } from '../models/dtos/cityDto';

@Injectable({
  providedIn: 'root'
})
export class CityService {
  private apiUrl = environment.apiUrl + "/cities";

  constructor(private http: HttpClient) {
  }

  getCities(): Observable<CityDto[]> {
    return this.http.get<CityDto[]>(this.apiUrl);
  }
}
