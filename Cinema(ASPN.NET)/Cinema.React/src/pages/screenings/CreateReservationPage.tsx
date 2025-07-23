import Form from "react-bootstrap/Form";
import { createReservation } from "@/api/client/reservation-client";
import { getScreeningById, getSeatsForScreening } from "@/api/client/screenings-client";
import { ReservationRequestDto, reservationRequestValidator } from "@/api/models/ReservationRequestDto";
import { ScreeningResponseDto } from "@/api/models/ScreeningResponseDto";
import { SubmitButton } from "@/components/buttons/SubmitButton";
import { ErrorAlert } from "@/components/alerts/ErrorAlert";
import { LoadingIndicator } from "@/components/LoadingIndicator";
import { SeatMap, SeatMapPosition } from "@/components/screenings/SeatMap";
import { ScreeningCard } from "@/components/screenings/ScreeningCard";
import React, { useEffect, useRef, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { SeatRequestDto } from "@/api/models/SeatRequestDto";
import * as yup from "yup";
import { FormError } from "@/components/FormError";
import { yupErrorsToObject } from "@/utils/forms";
import { ServerSideValidationError } from "@/api/errors/ServerSideValidationError";
import { HttpError } from "@/api/errors/HttpError";
import { getUserById } from "@/api/client/users-client.ts";
import { useUserContext } from "@/contexts/UserContext.ts";
import { HubConnection, HubConnectionBuilder, IHttpConnectionOptions, LogLevel } from "@microsoft/signalr";
import { SeatNotificationDto, SeatNotificationStatus } from "@/signalr/models/SeatNotificationDto";
import { accessTokenFactory } from "@/signalr/accessTokenFactory.ts";

/**
 * Shows the create reservation page with a seatmap
 * @constructor
 */
export function CreateReservationPage() {
    const userContext = useUserContext();
    const params = useParams();
    const navigate = useNavigate();
    const screeningId = params.screeningId ? parseInt(params.screeningId) : null;

    // Store the SignalR connection in a ref, so changing it won't trigger a rerender
    const signalRConnectionRef = useRef<HubConnection | null>(null);

    // State
    const [screening, setScreening] = useState<ScreeningResponseDto | null>(null);
    const [takenSeats, setTakenSeats] = useState<SeatMapPosition[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const [formErrors, setFormErrors] = useState<Record<string, string>>({});
    const [newReservation, setNewReservation] = useState<ReservationRequestDto>({
        screeningId: 0,
        name: "",
        email: "",
        phone: "",
        comment: "",
        seats: [],
    });

    useEffect(() => {
        async function loadContent() {
            if (!screeningId || !userContext.userId) {
                return;
            }

            setError(null);
            setIsLoading(true);
            try {
                const [loadedScreening, loadedSeats, user] = await Promise.all([
                    getScreeningById(screeningId),
                    getSeatsForScreening(screeningId),
                    getUserById(userContext.userId)
                ]);
                setScreening(loadedScreening);
                setTakenSeats(loadedSeats);

                setNewReservation(prevState => ({
                    ...prevState,
                    screeningId: screeningId,
                    name: user.name,
                    email: user.email,
                    phone: user.phoneNumber,
                }));
            } catch (e) {
                setError(e instanceof Error ? e.message : "Unknown error.");
            } finally {
                setIsLoading(false);
            }
        }

        async function initSignalRConnection() {
            const signalRConnection = new HubConnectionBuilder()
                .withUrl(`${import.meta.env.VITE_APP_SIGNALR_BASEURL}/ScreeningsHub`, {
                    accessTokenFactory: accessTokenFactory,
                } as IHttpConnectionOptions)
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Information)
                .build();

            signalRConnection.on("SeatStatusChanged", (_notificationScreeningId: number, notification: SeatNotificationDto) =>  {
                setTakenSeats(prevState => {
                    return notification.status === "None"
                        ? prevState.filter(s => s.row !== notification.row || s.column !== notification.column)
                        : [{ row: notification.row, column: notification.column }, ...prevState];
                });
            });
            
            await signalRConnection.start();
            await signalRConnection.invoke("JoinScreeningGroup", screeningId);
            signalRConnectionRef.current = signalRConnection;
        }

        loadContent()
            .then(() => initSignalRConnection());
        
        // Leave group and close connection when the user leaves the page
        return () => {
            signalRConnectionRef.current
                ?.invoke("LeaveScreeningGroup", screeningId)
                .then(() => signalRConnectionRef.current?.stop())
                .then(() => signalRConnectionRef.current = null);
        };
    }, [screeningId, userContext]);

    function handleInputChange(e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) {
        setNewReservation(prevState => ({
            ...prevState,
            [e.target.name]: e.target.value,
        }));
    }

    function handleSendSeatNotification(row: number, column: number, status: SeatNotificationStatus) {
        if (signalRConnectionRef.current) {
            const notification: SeatNotificationDto = {
                row,
                column,
                status,
            };
            signalRConnectionRef.current.invoke("SeatStatusChanged", screeningId, notification);
        }
    }

    function handleSeatMapChange(selectedSeat: SeatRequestDto) {
        setNewReservation(prevState => {
            // Remove seat selection
            const existingSeat = prevState.seats.find(s => s.row === selectedSeat.row && s.column === selectedSeat.column);
            if (existingSeat) {
                handleSendSeatNotification(selectedSeat.row, selectedSeat.column, "None");
                return {
                    ...prevState,
                    seats: prevState.seats.filter(s => s !== existingSeat)
                };
            }

            // If a new seat is added check the upper limit of seats
            const maxSeats = parseInt(import.meta.env.VITE_APP_MAX_SEATS, 10);
            if (prevState.seats.length === maxSeats) {
                return prevState;
            }

            // Add new seat
            handleSendSeatNotification(selectedSeat.row, selectedSeat.column, "Selected");
            return {
                ...prevState,
                seats: [...prevState.seats, selectedSeat]
            };
        });
    }

    async function handleFormSubmit(evt: React.FormEvent<HTMLFormElement>) {
        // Prevent default form submit behavior
        evt.preventDefault();

        // Submit the data to the backend
        setError(null);
        setFormErrors({});
        setIsLoading(true);
        try {
            reservationRequestValidator.validate(newReservation, { abortEarly: false });

            await createReservation(newReservation);
            return navigate("/reservations", {
                replace: true,
                state: { success: "Reservation created successfully." }
            });
        } catch (e) {
            if (e instanceof yup.ValidationError) {
                setFormErrors(yupErrorsToObject(e.inner));
            } else if (e instanceof ServerSideValidationError) {
                setFormErrors(e.validationErrors);
            } else if (e instanceof HttpError) {
                setError(e.message);
            } else {
                setError("Unknown error")
            }
        } finally {
            setIsLoading(false);
        }
    }

    // Render
    if (isLoading) {
        return <LoadingIndicator />
    }

    return (
        <>
            {error ? <ErrorAlert message={error} /> : null}
            {screening ? (
                <>
                    <h1>Create reservation</h1>
                    <ScreeningCard screening={screening} showMovieDetails/>
                    <h2>Reservation details</h2>
                    {/* Disable default HTML validation, because we use Yup */}
                    <Form onSubmit={handleFormSubmit} validated={false}>
                        <Form.Group className="mb-3">
                            <Form.Label htmlFor="name">Name:</Form.Label>
                            <Form.Control
                                type="text"
                                className="form-control"
                                id="name"
                                name="name"
                                onChange={handleInputChange}
                                value={newReservation.name}
                            />
                            <FormError message={formErrors.name}/>
                        </Form.Group>

                        <Form.Group className="mb-3">
                            <Form.Label htmlFor="phone">Phone:</Form.Label>
                            <Form.Control
                                type="tel"
                                className="form-control"
                                id="phone"
                                name="phone"
                                onChange={handleInputChange}
                                value={newReservation.phone}
                            />
                            <FormError message={formErrors.phone}/>
                        </Form.Group>

                        <Form.Group className="mb-3">
                            <Form.Label htmlFor="email">Email:</Form.Label>
                            <Form.Control
                                type="email"
                                className="form-control"
                                id="email"
                                name="email"
                                onChange={handleInputChange}
                                value={newReservation.email}
                            />
                            <FormError message={formErrors.email}/>
                        </Form.Group>

                        <Form.Group className="mb-3">
                            <Form.Label htmlFor="comment">Comment <i>(optional):</i></Form.Label>
                            <Form.Control
                                as="textarea"
                                id="comment"
                                name="comment"
                                rows={3}
                                onChange={handleInputChange}
                                value={newReservation.comment}
                            />
                            <FormError message={formErrors.comment}/>
                        </Form.Group>

                        <Form.Group className="mb-3">
                            <Form.Label>Seats:</Form.Label>
                            <SeatMap
                                rows={screening.room.rows}
                                columns={screening.room.columns}
                                takenSeats={takenSeats}
                                selectedSeats={newReservation.seats}
                                onSelectionChange={handleSeatMapChange}
                            />
                            <div className="d-flex justify-content-between ">
                                <FormError message={formErrors.seats}/>
                                <Form.Text>
                                    Selected seats: <i>{newReservation.seats.length} / {import.meta.env.VITE_APP_MAX_SEATS}</i>
                                </Form.Text>
                            </div>
                        </Form.Group>

                        <SubmitButton/>
                    </Form>
                </>
            ): null}
        </>
    );
}