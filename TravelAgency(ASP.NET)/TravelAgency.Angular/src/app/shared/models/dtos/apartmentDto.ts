import { DayOfWeekDto } from './dayOfWeekDto';

export interface ApartmentDto {
  id: number;
  room: string;
  comment: string;
  turnday: DayOfWeekDto;
  price: number;
}
