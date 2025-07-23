
export interface SeatNotificationDto {
    row: number;
    column: number;
    status: SeatNotificationStatus;
}

export type SeatNotificationStatus =  "None" | "Selected" | "Reserved" | "Sold";