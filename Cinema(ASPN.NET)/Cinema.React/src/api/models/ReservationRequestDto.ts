import * as yup from "yup";
import { SeatRequestDto } from "@/api/models/SeatRequestDto";
import { phoneRegex } from "@/utils/regex";

export interface ReservationRequestDto {
    name: string;
    phone: string;
    email: string;
    comment?: string;
    screeningId: number;
    seats: SeatRequestDto[];
}

export const reservationRequestValidator = yup.object({
    name: yup.string().required("Name is required"),
    email: yup.string().email().required("Email is required"),
    phone: yup.string()
        .matches(phoneRegex, 'Phone number can only contain numbers and may start with a +')
        .required("Phone is required"),
    comment: yup.string().optional(),
    seats: yup.array().min(1, "At load one seat must be selected").required()
});
