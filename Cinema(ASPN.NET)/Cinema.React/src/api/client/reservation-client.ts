import { ReservationRequestDto } from "@/api/models/ReservationRequestDto";
import { deleteResource, get, postAsJson } from "@/api/client/http";
import { ReservationResponseDto } from "@/api/models/ReservationResponseDto";

export async function getReservations() {
    return await get<ReservationResponseDto[]>("reservations");
}

export async function createReservation(body: ReservationRequestDto): Promise<ReservationResponseDto> {
    return await postAsJson<ReservationRequestDto, ReservationResponseDto>("reservations", body);
}

export async function cancelReservation(id: number): Promise<void> {
    await deleteResource(`reservations/${id}`);
}