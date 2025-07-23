export interface RentDto {
  id?: number | null;
  apartmentId: number;
  startDate: Date;
  endDate: Date;
  name: string;
  email: string;
  address: string;
  phoneNumber: string;
  totalPrice: number;
}
