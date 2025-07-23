import { SafeUrl } from '@angular/platform-browser';

import { ApartmentDto } from './apartmentDto';
import { CityDto } from './cityDto';
import { FeatureDto } from './featureDto';
import { ImageDto } from './imageDto';
import { ShoreTypeDto } from './shoreTypeDto';

export interface BuildingDto {
  id: number;
  name: string;
  city: CityDto;
  seaDistance: number;
  shore: ShoreTypeDto;
  features: FeatureDto[];
  locationX: number;
  locationY: number;
  comment: string;
  images: ImageDto[];
  mainImage: SafeUrl;
  apartments: ApartmentDto[];
}
