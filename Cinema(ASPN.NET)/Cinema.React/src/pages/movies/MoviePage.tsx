import { getMovie } from "@/api/client/movies-client";
import { getFutureScreenings } from "@/api/client/screenings-client";
import { MovieResponseDto } from "@/api/models/MovieResponseDto";
import { ScreeningResponseDto } from "@/api/models/ScreeningResponseDto";
import { ErrorAlert } from "@/components/alerts/ErrorAlert";
import { Base64Image } from "@/components/Base64Image";
import { LoadingIndicator } from "@/components/LoadingIndicator";
import { ScreeningCard } from "@/components/screenings/ScreeningCard";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

/**
 * Shows the details of a movie and its future screenings
 * @constructor
 */
export function MoviePage() {
    const params = useParams();
    const movieId = params.movieId ? parseInt(params.movieId) : null;
    const [movie, setMovie] = useState<MovieResponseDto | null>(null);
    const [screenings, setScreenings] = useState<ScreeningResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    
    useEffect(() => {
        async function loadContent() {
            if (!movieId) {
                return;
            }
            
            setError(null);
            setIsLoading(true);
            try {
                const [loadedMovie, loadedScreenings] = await Promise.all([
                    getMovie(movieId),
                    getFutureScreenings(movieId)
                ]);
                setMovie(loadedMovie);
                setScreenings(loadedScreenings);
            } catch (e) {
                setError(e instanceof Error ? e.message : "Unknown error.");
            } finally {
                setIsLoading(false);
            }
        }

        loadContent();
    }, [movieId]);

    // Render
    if (isLoading) {
        return <LoadingIndicator />;
    }

    return (
        <>
            {error ? <ErrorAlert message={error} /> : null}
            {movie ? (
                <div className="row">
                    <div className="col-md-3">
                        <Base64Image className="img-fluid" data={movie.image} alt={movie.title}/>
                    </div>
                    <div className="col-md-9">
                        <h1>{movie?.title}</h1>
                        <ul className="list-unstyled">
                            <li><strong>Released:</strong> {movie.year}</li>
                            <li><strong>Directed by:</strong> {movie.director}</li>
                            <li><strong>Length:</strong> {movie.length} minutes</li>
                        </ul>
                        <p>{movie?.synopsis}</p>
                    </div>
                    <hr className="mt-4"/>
                    <div>
                        <h2 className="mt-0">Screenings</h2>
                        {screenings.map(screening =>
                            <ScreeningCard
                                key={screening.id}
                                screening={screening}
                                showReservationLink
                            />
                        )}
                    </div>
                </div>
            ) : null}
        </>
    );
}