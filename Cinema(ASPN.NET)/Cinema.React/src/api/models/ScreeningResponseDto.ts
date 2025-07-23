import { MovieResponseDto } from "@/api/models/MovieResponseDto";
import { RoomResponseDTO } from "@/api/models/RoomResponseDto";

export interface ScreeningResponseDto {
    id: number;
    movie: MovieResponseDto;
    room: RoomResponseDTO;
    startsAt: string;
    price: number;
}