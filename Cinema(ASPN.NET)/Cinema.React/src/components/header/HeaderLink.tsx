import { NavLink } from "react-router-dom";

interface Props {
    text: string,
    to: string;
}

/**
 * Wraps the NavLink from React Router and adds Bootstrap classes based on the currently selected route
 * @param text
 * @param to
 * @constructor
 */
export function HeaderLink({ text, to }: Props) {
    return (
        <li className="nav-item">
            <NavLink
                to={to}
                className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}
            >
                {text}
            </NavLink>
        </li>
    );
}