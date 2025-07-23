import { Link } from "react-router-dom";

interface Props {
    to: string;
    text: string;
}

/**
 * A React Router link styled as a Bootstrap button
 * @param to
 * @param text
 * @constructor
 */
export function LinkButton({ to, text }: Props) {
    return <Link className="btn btn-primary" to={to}>{text}</Link>
}