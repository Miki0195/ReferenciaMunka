import Card from 'react-bootstrap/Card';
import { ScreeningResponseDto } from "@/api/models/ScreeningResponseDto";
import { LinkButton } from "@/components/buttons/LinkButton";
import { Link } from "react-router-dom";
import { stringToLocaleDateTime } from "@/utils/date";

interface Props {
    screening: ScreeningResponseDto;
    showMovieDetails?: boolean;
    showReservationLink?: boolean
}

export function ScreeningCard({ screening, showMovieDetails = false, showReservationLink = false }: Props) {
    return (
        <Card className="my-3">
            {showMovieDetails ? (
                <Card.Header>
                    <h3 className="my-0">
                        <Link className="text-dark text-underline-hover" to={`/movies/${screening.movie.id}`} >
                            {screening.movie.title}
                        </Link>
                    </h3>
                </Card.Header>
            ) : null}
            <Card.Body>
                <ul className="list-unstyled m-0">
                    <li><strong>Starts at:</strong> {stringToLocaleDateTime(screening.startsAt)}</li>
                    {showMovieDetails ? <li><strong>Length:</strong> {screening.movie.length} minutes</li> : null}
                    <li><strong>Room:</strong> {screening.room.name}</li>
                    <li><strong>Price per ticket:</strong> {screening.price}</li>
                </ul>
            </Card.Body>
            {showReservationLink ? (
                <Card.Footer>
                    <LinkButton to={`/screenings/${screening.id}/create-reservation`} text="Reserve" />
                </Card.Footer>
            ) : null}
            
        </Card>
    );
}