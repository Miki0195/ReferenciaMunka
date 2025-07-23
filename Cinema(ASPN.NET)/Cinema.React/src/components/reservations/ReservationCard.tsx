import Card from "react-bootstrap/Card";
import Button from "react-bootstrap/Button";
import { ReservationResponseDto } from "@/api/models/ReservationResponseDto";
import { SeatMap } from "@/components/screenings/SeatMap";
import { useState } from "react";
import { stringToLocaleDateTime } from "@/utils/date";

interface Props {
    reservation: ReservationResponseDto,
    onCancel: () => void,
};

export function ReservationCard({ reservation, onCancel } : Props) {
    const [showDetails, setDetails] = useState<boolean>(false);
    
    function handleDetailsToggle() {
        setDetails(prevState => !prevState);
    }
    
    const canCancel = new Date(reservation.screening.startsAt) >= new Date();
    
    return (
        <Card className="my-3">
            <Card.Header>
                <h3 className="my-0">{reservation.screening.movie.title}</h3>
            </Card.Header>
            <Card.Body>
                <ul className="list-unstyled m-0">
                    <li><strong>Room:</strong> {reservation.screening.room.name}</li>
                    <li><strong>Starts at:</strong> {stringToLocaleDateTime(reservation.screening.startsAt)}</li>
                    <li><strong>Total price:</strong> {reservation.screening.price * reservation.seats.length}</li>
                </ul>
                {showDetails ? (
                    <>
                        <hr/>
                        <h4 className="mt-0">Reservation details</h4>
                        <ul className="list-unstyled m-0">
                            <li><strong>Reservation #:</strong> {reservation.id}</li>
                            <li><strong>Created at:</strong> {stringToLocaleDateTime(reservation.createdAt)}</li>
                            <li><strong>Name:</strong> {reservation.name}</li>
                            <li><strong>Phone:</strong> {reservation.phone}</li>
                            <li><strong>Email:</strong> {reservation.email}</li>
                            <li><strong>Comment:</strong> {reservation.comment}</li>
                        </ul>

                        <hr/>
                        <h4 className="mt-0">Seats</h4>
                        <SeatMap
                            rows={reservation.screening.room.rows}
                            columns={reservation.screening.room.columns}
                            selectedSeats={reservation.seats}
                        />
                    </>
                ) : null}
            </Card.Body>
            <Card.Footer>
                <Button variant="primary" onClick={handleDetailsToggle}>
                    {showDetails ? "Hide details" : "Show details"}
                </Button>
                {canCancel ? <Button variant="danger" className="ms-1" onClick={onCancel}>Cancel</Button> : null}
            </Card.Footer>
        </Card>
    )
}