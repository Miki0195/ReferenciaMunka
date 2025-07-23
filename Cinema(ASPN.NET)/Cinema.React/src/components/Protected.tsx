import { ReactNode } from "react";
import {useUserContext} from "@/contexts/UserContext";
import { Navigate, useLocation } from "react-router-dom";

interface Props {
    children: ReactNode;
}

export function Protected({ children }: Props) {
    const userContext = useUserContext();
    const location = useLocation();
    
    return userContext.loggedIn 
        ? children 
        : <Navigate to="/user/login" state={{ loginRedirect: location.pathname }} />;
}