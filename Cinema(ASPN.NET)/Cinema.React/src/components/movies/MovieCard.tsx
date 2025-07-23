import Card from "react-bootstrap/Card";
import Col from "react-bootstrap/Col";
import { MovieResponseDto } from "@/api/models/MovieResponseDto"
import { Link } from "react-router-dom";
import { Base64CardImage } from "@/components/Base64CardImage";

interface Props {
    movie: MovieResponseDto;
}


export function MovieCard({ movie }: Props) {
    return (
        <Col>
            <Card className="h-100">
                <Base64CardImage data={movie.image} alt={movie.title} variant="top" />
                <Card.Body>
                    <h5 className="card-title">
                        <Link className="stretched-link text-dark text-underline-hover" to={`/movies/${movie.id}`}>{movie.title}</Link>
                    </h5>
                </Card.Body>
            </Card>
        </Col>
    )
}