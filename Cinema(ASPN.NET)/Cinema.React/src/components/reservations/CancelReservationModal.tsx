import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import { ReservationResponseDto } from "@/api/models/ReservationResponseDto";

interface Props {
    show: boolean;
    reservation: ReservationResponseDto | null,
    onConfirm: () => void,
    onHide: () => void,
}

export function CancelReservationModal({ show, reservation, onConfirm, onHide }: Props) {
    return (
        <Modal show={show} onHide={onHide}>
            <Modal.Header closeButton>
                <Modal.Title>Cancel Reservation</Modal.Title>
            </Modal.Header>
            
            <Modal.Body>
                Do you really want to cancel reservation <strong>#{reservation?.id}</strong> for <strong>{reservation?.screening.movie.title}</strong>?
            </Modal.Body>

            <Modal.Footer>
                <Button variant="danger" onClick={onConfirm}>Yes</Button>
                <Button variant="secondary" onClick={onHide}>No</Button>
            </Modal.Footer>
        </Modal>  
    );
}