import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { GoogleMapsModule } from '@angular/google-maps';
import { MatButton } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatGridList, MatGridTile } from '@angular/material/grid-list';
import { MatList, MatListItem } from '@angular/material/list';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApartmentDto } from '@shared/models/dtos/apartmentDto';
import { BuildingDto } from '@shared/models/dtos/buildingDto';
import { ImageDto } from '@shared/models/dtos/imageDto';
import { FeaturePipe } from '@shared/pipes/FeaturePipe';
import { ShoreTypePipe } from '@shared/pipes/ShoreTypePipe';
import { BuildingService } from '@shared/services/building.service';
import { GoogleMapsService } from '@shared/services/googlemaps.service';
import { UserService } from '@shared/services/user.service';
import { map, Observable, shareReplay } from 'rxjs';

import { ApartmentRentComponent } from '../apartment-rent/apartment-rent.component';

@Component({
  selector: 'app-building-details',
  templateUrl: './building-details.component.html',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatList,
    MatListItem,
    MatGridList,
    MatGridTile,
    MatSelectModule,
    MatProgressSpinner,
    MatButton,
    GoogleMapsModule,
    ShoreTypePipe,
    FeaturePipe,
    ApartmentRentComponent,
    RouterLink
  ],
  styleUrls: ['./building-details.component.css']
})
export class BuildingDetailsComponent implements OnInit {
  building?: BuildingDto;
  images: SafeResourceUrl [] = [];
  imagesLoaded = false;
  loggedIn = true;
  currentUrl!: string;

  destroyRef = inject(DestroyRef);
  map$!: Observable<boolean>;
  loadingMap!: Observable<boolean>;
  zoom = 12;
  center!: google.maps.LatLngLiteral;
  options: google.maps.MapOptions = {
    mapTypeId: 'hybrid',
  };

  selectedApartment?: ApartmentDto | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private buildingService: BuildingService,
    private googleMapsService: GoogleMapsService,
    private userService: UserService,
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      const buildingId = params['id'];
      if (buildingId) {
        this.fetchBuildingDetails(+buildingId);
      }
    });
    this.currentUrl = this.router.url;
    this.map$ = this.googleMapsService.loadApi().pipe(takeUntilDestroyed(this.destroyRef), shareReplay());
    this.loadingMap = this.map$.pipe(map((obj) => !!obj));

    this.loggedIn = this.userService.isLoggedIn();
    this.userService.loggedInStatusChanged$.subscribe(status => this.loggedIn = status);
  }

  fetchBuildingDetails(id: number) {
    this.buildingService.getBuildingById(id).subscribe({
      next: (data: BuildingDto) => {
        this.building = data;
        this.building.features = data.features.filter(f => f.isAvailable);

        this.center = { lat: data.locationX, lng: data.locationY };

        this.buildingService.getImages(data.id).subscribe({
          next: (images: ImageDto[]) => {
            this.imagesLoaded = true;
            images.forEach((image: ImageDto) => {
              const url = 'data:image/png;base64,' + image.imageLarge;
              this.images.push(url);
            });
          }
        });
      },
      error: (error) => {
        console.error('Error fetching building:', error);
      }
    });
  }

  rentApartment(apartment: ApartmentDto) {
    this.selectedApartment = apartment;
  }

  closeRentComponent() {
    this.selectedApartment = null;
  }
}
