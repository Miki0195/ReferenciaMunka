import Row from "react-bootstrap/Row"
import { MovieResponseDto } from "@/api/models/MovieResponseDto";
import { MovieCard } from "@/components/movies/MovieCard";

interface Props {
    movies: MovieResponseDto[];
}

export function MoviesGrid({ movies } : Props) {
    return (
        <Row xs={1} md={5} className="g-4">
            {movies.map(movie => <MovieCard key={movie.id} movie={movie} />)}
        </Row>
    )
}