import { LoadingIndicator } from "@/components/LoadingIndicator";
import { useUserContext } from "@/contexts/UserContext";
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";

export function LogoutPage() {
    const userContext = useUserContext();
    const navigate = useNavigate();

    useEffect(() => {
        userContext.handleLogout()
            .then(() => navigate("/"));
    }, [userContext, navigate]);
    
    return <LoadingIndicator />;
}