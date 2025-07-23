import { get } from "@/api/client/http";
import { MovieResponseDto } from "@/api/models/MovieResponseDto";

export async function getMovies(count?: number): Promise<MovieResponseDto[]> {
    return await get<MovieResponseDto[]>("movies", count ? { count: count.toString() } : undefined);
}

export async function getMovie(id: number): Promise<MovieResponseDto> {
    return await get<MovieResponseDto>(`movies/${id}`);
}