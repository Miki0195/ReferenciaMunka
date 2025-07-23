import { ErrorAlert } from "@/components/alerts/ErrorAlert"

/**
 * Fallback page of the current url does not match any route
 * @constructor
 */
export function NotFoundPage() {
    return <ErrorAlert message="Page not found" />
}