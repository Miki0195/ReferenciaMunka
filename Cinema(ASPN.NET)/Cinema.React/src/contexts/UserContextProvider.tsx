import { ReactNode, useState, useEffect, useCallback } from "react";
import { LoginRequestDto } from "@/api/models/LoginRequestDto";
import { login, logout, refresh } from "@/api/client/users-client";
import { getJwtExpiration } from "@/utils/jwt";
import { LoginResponseDto } from "@/api/models/LoginResponseDto";
import { HttpError } from "@/api/errors/HttpError";
import { UserContext, UserContextModel, UserInfo } from "@/contexts/UserContext";

export function UserContextProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<UserInfo | null>(null);
    const [authError, setAuthError] = useState<string | null>(null);
    const [initialized, setInitialized] = useState<boolean>(false);
    const loggedIn = user !== null;

    const handleLoginResponse = useCallback((response: LoginResponseDto)=> {
        const user: UserInfo = {
            userId: response.userId,
            authToken: response.authToken,
            refreshToken: response.refreshToken,
            authTokenExpiration: getJwtExpiration(response.authToken),
        };
        localStorage.setItem("user", JSON.stringify(user));
        setUser(user);
    }, []);

    const redeemToken = useCallback(async (refreshToken: string) => {
        try {
            const response = await refresh(refreshToken);
            handleLoginResponse(response);
        } catch (e) {
            // Auto logout user if the refresh token was invalid
            // Otherwise, show an error message, e.g. the server is not available
            if (e instanceof HttpError && e.status === 403) {
                localStorage.removeItem("user");
                setUser(null);
            } else if (e instanceof Error) {
                setAuthError(e.message);
            } else {
                setAuthError("Unknown error.");
            }
        }
    }, [handleLoginResponse]);
    
    const handleLogin = useCallback(async (data: LoginRequestDto) => {
        const response = await login(data);
        handleLoginResponse(response);
    }, [handleLoginResponse]);
    
    const handleLogout = useCallback(async () => {
        if (!loggedIn) {
            return;
        }
        await logout();
        localStorage.removeItem("user");
        setUser(null);
    }, [loggedIn]);

    useEffect(() => {
        async function initialize() {
            try {
                const userSessionItem = localStorage.getItem("user");
                if (!userSessionItem) {
                    return;
                }

                const userFromSession = JSON.parse(userSessionItem) as UserInfo;
                if (userFromSession.authTokenExpiration + 60_000 > Date.now()) {
                    // Refresh token if it is expired or it is going to expire in one minute
                    await redeemToken(userFromSession.refreshToken);
                } else {
                    // Otherwise load the existing tokens from the session
                    setUser(userFromSession);
                }
            } finally {
                setInitialized(true);
            }
        }

        initialize();
    }, [redeemToken]);

    useEffect(() => {
        if (!user) {
            return;
        }

        // Refresh 1 minute before the token expires
        const timeRemaining = user.authTokenExpiration - Date.now();
        const timeoutDuration = timeRemaining - 60_000;
        const timeoutId = setTimeout(async () => {
            await redeemToken(user.refreshToken);
        }, timeoutDuration);

        return () => clearTimeout(timeoutId);
    }, [user, redeemToken]);
    
    const contextValue: UserContextModel = {
        userId: user ? user.userId : null,
        loggedIn,
        initialized,
        authError,
        handleLogin,
        handleLogout,
    }

    return (
        <UserContext.Provider value={contextValue}>
            {children}
        </UserContext.Provider>
    )
}