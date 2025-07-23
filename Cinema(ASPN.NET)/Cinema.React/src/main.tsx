import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import 'bootstrap/dist/css/bootstrap.css';
import '@/index.css';

import { RootLayout } from "@/pages/RootLayout";
import { HomePage } from "@/pages/HomePage";
import { MoviesPage } from "@/pages/movies/MoviesPage";
import { MoviePage } from "@/pages/movies/MoviePage";
import { CreateReservationPage } from "@/pages/screenings/CreateReservationPage";
import { LoginPage} from "@/pages/user/LoginPage";
import { LogoutPage } from "@/pages/user/LogoutPage";
import { RegisterPage } from "@/pages/user/RegisterPage";
import { ReservationsPage } from "@/pages/reservations/ReservationsPage";
import { NotFoundPage } from "@/pages/NotFoundPage";
import { UserContextProvider } from "@/contexts/UserContextProvider";
import { Protected } from "@/components/Protected";

const router = createBrowserRouter([
    {
        element: <RootLayout />,
        children: [
            {
                path: "/",
                element: <HomePage />
            },
            {
                path: "/movies",
                element: <MoviesPage />,
            },
            {
                path: "/movies/:movieId",
                element: <MoviePage />,
            },
            {
                path: "/screenings/:screeningId/create-reservation",
                element: <Protected><CreateReservationPage /></Protected>,
            },
            {
                path: "/reservations",
                element: <Protected><ReservationsPage /></Protected>,
            },
            {
                path: "/user/login",
                element: <LoginPage />,
            },
            {
                path: "/user/logout",
                element: <Protected><LogoutPage /></Protected>,
            },
            {
                path: "/user/register",
                element: <RegisterPage />,
            },
            {
                path: "*",
                element: <NotFoundPage />
            },
        ]
    },
]);

createRoot(document.getElementById('root')!).render(
  <UserContextProvider>
    <RouterProvider router={router} />
  </UserContextProvider>
);
