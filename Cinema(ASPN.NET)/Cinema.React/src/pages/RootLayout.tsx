import Container from "react-bootstrap/Container";
import { Header } from "@/components/header/Header";
import { RouterSuccessAlert } from "@/components/alerts/RouterSuccessAlert";
import { Outlet } from "react-router-dom";
import { useUserContext } from "@/contexts/UserContext";
import { LoadingIndicator } from "@/components/LoadingIndicator";
import { ErrorAlert } from "@/components/alerts/ErrorAlert";

/**
 * The root layout with navigation header
 * @constructor
 */
export function RootLayout() {
    const userContext = useUserContext();
    
    // Do not load content before the tokens are loaded after application startup
    if (!userContext.initialized) {
        return <LoadingIndicator />;
    }
    
    // If there was an unexpected error during token refresh show error message
    if (userContext.authError) {
        return <Container className="my-4">
            <ErrorAlert message={`Failed to authenticate: ${userContext.authError}`} />
        </Container>;
    }
    
    return (
        <>
            <Header />
            <Container className="my-4">
                <RouterSuccessAlert />

                {/* Outlet is a component form React Router that renders the currently matched page */}
                <Outlet />
            </Container>
        </>
    );
}