import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { DomSanitizer } from '@angular/platform-browser';
import { Router, RouterModule } from '@angular/router';
import { BuildingDto } from '@shared/models/dtos/buildingDto';
import { CityDto } from '@shared/models/dtos/cityDto';
import { BuildingService } from '@shared/services/building.service';
import { CityService } from '@shared/services/city.service';

@Component({
  selector: 'app-building-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatGridListModule,
    MatSelectModule,
    MatSelectModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './building-list.component.html',
  styleUrls: ['./building-list.component.css'],
})
export class BuildingListComponent implements OnInit {
  buildings: BuildingDto[] = [];
  cities: CityDto[] = [];
  selectedCity?: CityDto| null = null;

  constructor(
    private buildingService: BuildingService,
    private cityService: CityService,
    private sanitizer: DomSanitizer,
    private router: Router) { }

  ngOnInit() {
    this.cityService.getCities().subscribe({
      next: (data: CityDto[]) => {
          this.cities = data;
          this.fetchBuildings(null);
      }
    })
  }

  onSelectionChange(event: MatSelectChange) {
    this.fetchBuildings(event.value);
  }

  fetchBuildings(city: CityDto | null) {
    this.selectedCity = city;

    this.buildingService.getBuildings(city?.id).subscribe({
      next: (data: BuildingDto[]) => {
        this.buildings = data;
        this.buildings.forEach(building => {
          this.buildingService.getMainImage(building.id).subscribe(image => {
            building.mainImage = this.sanitizer.bypassSecurityTrustUrl(URL.createObjectURL(image));
          });
        });
      },
      error: (error) => {
        console.error('Error fetching buildings:', error);
      }
    });
  }
}
