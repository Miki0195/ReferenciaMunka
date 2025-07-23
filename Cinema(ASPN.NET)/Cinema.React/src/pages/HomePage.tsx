import { getMovies } from "@/api/client/movies-client";
import { getTodaysScreenings } from "@/api/client/screenings-client";
import { MovieResponseDto } from "@/api/models/MovieResponseDto";
import { ScreeningResponseDto } from "@/api/models/ScreeningResponseDto";
import { ErrorAlert } from "@/components/alerts/ErrorAlert";
import { LoadingIndicator } from "@/components/LoadingIndicator";
import { MoviesGrid } from "@/components/movies/MoviesGrid";
import { ScreeningCard } from "@/components/screenings/ScreeningCard";
import { useEffect, useState } from "react";

const NUMBER_OF_LATEST_MOVIES = 5;

/**
 * Home page that shows the latest movies and the screenings for today
 * @constructor
 */
export function HomePage() {
    const [movies, setMovies] = useState<MovieResponseDto[]>([]);
    const [screenings, setScreenings] = useState<ScreeningResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function loadContent() {
            setError(null);
            setIsLoading(true);
            try {
                const [loadedMovies, loadedScreenings] = await Promise.all([
                    getMovies(NUMBER_OF_LATEST_MOVIES),
                    getTodaysScreenings()
                ]);
                setMovies(loadedMovies);
                setScreenings(loadedScreenings);
            } catch (e) {
                setError(e instanceof Error ? e.message : "Unknown error.");
            } finally {
                setIsLoading(false);
            }
        }
        
        loadContent();
    }, []);

    // Render
    if (isLoading) {
        return <LoadingIndicator />;
    }

    return (
    <>
        {error ? <ErrorAlert message={error} /> : null}
        <h1>Welcome to ELTE Cinema!</h1>
        
        <h2>Latest movies</h2>
        <MoviesGrid movies={movies} />
        <h2>Today's program</h2>
        {screenings.map(screening => <ScreeningCard
            key={screening.id}
            showMovieDetails
            showReservationLink
            screening={screening}
        />)}
    </>);
}