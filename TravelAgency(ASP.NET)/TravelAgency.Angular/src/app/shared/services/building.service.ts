import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@environments/environment';
import { Observable } from 'rxjs';

import { BuildingDto } from '../models/dtos/buildingDto';
import { ImageDto } from '../models/dtos/imageDto';

@Injectable({
  providedIn: 'root'
})
export class BuildingService {
  private apiUrl = environment.apiUrl + "/buildings";

  constructor(private http: HttpClient) { }

  getBuildings(cityId?: number | null): Observable<BuildingDto[]> {
    let params = new HttpParams();
    if (cityId) {
      params = params.set('cityId', cityId);
    }

    return this.http.get<BuildingDto[]>(`${this.apiUrl}`, { params: params });
  }

  getBuildingById(buildingId: number): Observable<BuildingDto> {
    return this.http.get<BuildingDto>(`${this.apiUrl}/${buildingId}`);
  }

  getImages(buildingId: number): Observable<ImageDto[]> {
    return this.http.get<ImageDto[]>(`${this.apiUrl}/${buildingId}/images`);
  }
  getMainImage(buildingId: number): Observable<Blob> {
    let params = new HttpParams();
    params = params.set('large', true);

    return this.http.get(`${this.apiUrl}/${buildingId}/images/main`, { params: params, responseType: 'blob' });
  }
}
