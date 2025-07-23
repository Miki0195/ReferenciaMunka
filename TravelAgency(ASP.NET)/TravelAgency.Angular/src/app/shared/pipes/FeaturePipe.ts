import { Pipe, PipeTransform } from '@angular/core';

import { FeatureDto } from '../models/dtos/featureDto';

@Pipe({
  standalone: true,
  name: 'feature'
})
export class FeaturePipe implements PipeTransform {
  transform(features: FeatureDto[]): string {

    const featureList: string[] = [];

    features.forEach(feature => {
      if (feature.id == 0)
        featureList.push("főút");
      else if (feature.id == 1)
        featureList.push("parti szolgálat");
      else if (feature.id == 2)
        featureList.push("úszómedence");
      else if (feature.id == 3)
        featureList.push("kert");
      else if (feature.id == 4)
        featureList.push("saját parkoló");
    });

    const featureString = featureList.join(", ");
    return featureString.charAt(0).toUpperCase() + featureString.slice(1);
  }
}
