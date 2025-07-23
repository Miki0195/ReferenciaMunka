import { useLocation } from "react-router-dom";
import Alert from "react-bootstrap/Alert";

/**
 * Reads the success message from the Router state and shows it exists
 * Useful for showing success messages after redirection
 * @constructor
 */
export function RouterSuccessAlert() {
    const location = useLocation();

    if (!location.state?.success) {
        return null;
    }

    return (
        <Alert variant="success">
            <Alert.Heading>Success</Alert.Heading>
            <p>{location.state?.success}</p>
        </Alert>
    );
}