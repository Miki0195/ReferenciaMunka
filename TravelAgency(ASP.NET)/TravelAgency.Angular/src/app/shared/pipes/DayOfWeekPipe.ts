import { Pipe, PipeTransform } from '@angular/core';

import { DayOfWeekDto } from '../models/dtos/dayOfWeekDto';

@Pipe({
  standalone: true,
  name: 'dayOfWeek'
})
export class DayOfWeekPipe implements PipeTransform {
  transform(value: DayOfWeekDto): string {
    switch (value) {
      case DayOfWeekDto.Sunday:
        return 'Vasárnap';
      case DayOfWeekDto.Monday:
        return 'Hétfő';
      case DayOfWeekDto.Tuesday:
        return 'Kedd';
      case DayOfWeekDto.Wednesday:
        return 'Szerda';
      case DayOfWeekDto.Thursday:
        return 'Csütörtök';
      case DayOfWeekDto.Friday:
        return 'Péntek';
      case DayOfWeekDto.Saturday:
        return 'Szombat';
      default:
        return 'Ismeretlen';
    }
  }
}
