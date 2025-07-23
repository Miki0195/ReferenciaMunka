import { Injectable } from '@angular/core';

import { DayOfWeekDto } from '../models/dtos/dayOfWeekDto';

@Injectable({
  providedIn: 'root'
})
export class DateUtility {
  static getNextDayOfWeek(date: Date, dayOfWeek: DayOfWeekDto): Date {
    const resultDate = new Date(date.getTime());
    const values = Object.values(DayOfWeekDto);
    const targetDay = values.indexOf(dayOfWeek);
    resultDate.setDate(date.getDate() + ((targetDay + 7 - date.getDay()) % 7));

    const currentDay = new Date();
    currentDay.setDate(currentDay.getDate() + 7);

    if(resultDate < currentDay){
      resultDate.setDate(resultDate.getDate() + 7);
    }
    return resultDate;
  }
  static notBooked(date: Date, dates: Date[]): boolean {
    return !dates.some(bookedDate =>
      bookedDate.getDate() === date.getDate() &&
      bookedDate.getMonth() === date.getMonth() &&
      bookedDate.getFullYear() === date.getFullYear());
  }

  static notBookedBetween(startDate: Date, endDate: Date, dates: Date[]): boolean {
    const currentDate = new Date(startDate);
    while(currentDate < endDate){
      if(!this.notBooked(currentDate, dates))
        return false;
      else
        currentDate.setDate(currentDate.getDate() + 7);
    }

    return true;
  }
}
