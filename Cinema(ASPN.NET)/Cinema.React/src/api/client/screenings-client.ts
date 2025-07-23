import { ScreeningResponseDto } from "@/api/models/ScreeningResponseDto";
import { SeatResponseDto } from "@/api/models/SeatResponseDto";
import { get } from "@/api/client/http";

export async function getTodaysScreenings(): Promise<ScreeningResponseDto[]> {
    const startsAfter = new Date();
    const startsBefore = new Date();
    startsBefore.setHours(23, 59, 59, 999);

    return await get("screenings", {
        startsAfter: startsAfter.toLocaleString(),
        startsBefore: startsBefore.toLocaleString()
    });
}

export async function getFutureScreenings(movieId: number): Promise<ScreeningResponseDto[]> {
    const startsAfter = new Date();
    
    return await get("screenings", {
        startsAfter: startsAfter.toLocaleString(),
        movieId: movieId.toString()
    });
}

export async function getScreeningById(id: number): Promise<ScreeningResponseDto> {
    return await get<ScreeningResponseDto>(`screenings/${id}`);
}

export async function getSeatsForScreening(id: number): Promise<SeatResponseDto[]> {
    return await get<SeatResponseDto[]>(`screenings/${id}/seats`);
}