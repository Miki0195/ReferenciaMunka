import { ScreeningResponseDto } from "@/api/models/ScreeningResponseDto";
import { SeatStatus } from "@/api/models//SeatStatus";

export interface SeatResponseDto {
    id: number;
    row: number;
    column: number;
    status: SeatStatus;
    screening?: ScreeningResponseDto;
}