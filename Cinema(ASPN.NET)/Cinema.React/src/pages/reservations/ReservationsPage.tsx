import {useEffect, useState} from "react";
import {cancelReservation, getReservations} from "@/api/client/reservation-client";
import {LoadingIndicator} from "@/components/LoadingIndicator";
import {ErrorAlert} from "@/components/alerts/ErrorAlert";
import {ReservationCard} from "@/components/reservations/ReservationCard";
import { ReservationResponseDto } from "@/api/models/ReservationResponseDto";
import { CancelReservationModal } from "@/components/reservations/CancelReservationModal";

export function ReservationsPage() {
    const [reservations, setReservations] = useState<ReservationResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [reservationToCancel, setReservationToCancel] = useState<ReservationResponseDto | null>(null);
    const [showReservationModal, setShowReservationCancelModal] = useState(false);

    useEffect(() => {
        async function loadData() {
            setIsLoading(true);
            setError(null);
            try {
                const loadedReservations = await getReservations();
                setReservations(loadedReservations);
            } catch (e) {
                setError(e instanceof Error ? e.message : "Unknown error.");
            } finally {
                setIsLoading(false);
            }
        }
        
        loadData();
    }, []);
    
    function handleReservationCancel(reservation: ReservationResponseDto) {
        setReservationToCancel(reservation);
        setShowReservationCancelModal(true);
    }
    
   async  function handleReservationCancelConfirmed() {
        if (!reservationToCancel) {
            return;
        }
        
        // Optimistic delete of the reservation locally
        setReservations(prevState => prevState.filter(r => r.id !== reservationToCancel.id));
        setShowReservationCancelModal(false);
        
        try {
            // Delete the reservation from the server
            await cancelReservation(reservationToCancel.id);
        } catch (e) {
            // Restore the original reservation.
            // The 'reservations' variable still contains the original list, because of closures.
            setReservations(reservations);
            
            // Show the error alert
            setError(e instanceof Error ? e.message : "Unknown error.");
        }
    }
    
    function handleReservationCancelModalHide() {
        setShowReservationCancelModal(false);
    }
    
    if (isLoading) {
        return <LoadingIndicator />;
    }
    
    return (
        <>
            {error ? <ErrorAlert message={error} /> : null}
            <h1>My reservations</h1>
            {reservations.map((reservation) => <ReservationCard
                key={reservation.id}
                reservation={reservation}
                onCancel={() => handleReservationCancel(reservation)}
            />)}
            <CancelReservationModal
                show={showReservationModal}
                reservation={reservationToCancel}
                onConfirm={handleReservationCancelConfirmed}
                onHide={handleReservationCancelModalHide}
            />
        </>
    );
}