import { SeatResponseDto } from "@/api/models/SeatResponseDto";
import { ScreeningResponseDto } from "@/api/models/ScreeningResponseDto";

export interface ReservationResponseDto {
    id: number;
    name: string;
    phone: string;
    email: string;
    createdAt: string;
    comment?: string;
    seats: SeatResponseDto[];
    screening: ScreeningResponseDto;
}
