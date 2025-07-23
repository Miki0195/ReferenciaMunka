import { Injectable } from '@angular/core';
import { environment } from '@environments/environment';
import { Loader } from '@googlemaps/js-api-loader';
import { defer,Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GoogleMapsService {
  private loaded = false;

  loadApi(): Observable<boolean> {
    if(this.loaded)
      return of(this.loaded);

    const loader = new Loader({
      apiKey: environment.googleMapsApiKey,
      version: 'weekly'
    });

    return defer(async() => {
      try {
        await loader.importLibrary('maps');
        this.loaded = true;
        return true;
      }
      catch(error){
        console.log(error);
        this.loaded = false;
        return false;
      }
    })
  }
}
