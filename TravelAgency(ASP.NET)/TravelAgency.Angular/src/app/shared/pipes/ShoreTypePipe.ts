import { Pipe, PipeTransform } from '@angular/core';

import { ShoreTypeDto } from '../models/dtos/shoreTypeDto';

@Pipe({
  standalone: true,
  name: 'shoreType'
})
export class ShoreTypePipe implements PipeTransform {
  transform(value: ShoreTypeDto): string {
    switch (value) {
      case ShoreTypeDto.Sandy:
        return 'Homokos';
      case ShoreTypeDto.Rocky:
        return 'Sziklás';
      case ShoreTypeDto.Gravelly:
        return 'Kavicsos';
      default:
        return 'Ismeretlen';
    }
  }
}
