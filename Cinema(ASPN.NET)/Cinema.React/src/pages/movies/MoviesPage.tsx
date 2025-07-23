import { getMovies } from "@/api/client/movies-client";
import { MovieResponseDto } from "@/api/models/MovieResponseDto";
import { ErrorAlert } from "@/components/alerts/ErrorAlert";
import { LoadingIndicator } from "@/components/LoadingIndicator";
import { MoviesGrid } from "@/components/movies/MoviesGrid";
import { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { MovieNotificationDto } from "@/signalr/models/MovieNotificationDto";

/**
 * Shows all movies
 * @constructor
 */
export function MoviesPage() {
    const [movies, setMovies] = useState<MovieResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function loadContent() {
            setError(null);
            setIsLoading(true);
            try {
                const loadedMovies = await getMovies();
                setMovies(loadedMovies);
            } catch (e) {
                setError(e instanceof Error ? e.message : "Unknown error.");
            } finally {
                setIsLoading(false);
            }
        }

        const signalRConnection = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_APP_SIGNALR_BASEURL}/MoviesHub`)
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();
        
        signalRConnection.on("NewMovieAdded", (newMovie: MovieNotificationDto) => {
            setMovies(prevState => [newMovie, ...prevState]);
        });
        
        loadContent()
            .then(() => signalRConnection.start());

        return () => {
            signalRConnection.stop();
        }
    }, []);

    // Render
    if (isLoading) {
        return <LoadingIndicator />;
    }
    
    return (
        <>
            {error ? <ErrorAlert message={error} /> : null}
            <h1>Movies</h1>
            <MoviesGrid movies={movies} />
        </>
    )
}