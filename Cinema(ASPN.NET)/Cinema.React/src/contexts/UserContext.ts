import { createContext, useContext } from "react";
import { LoginRequestDto } from "@/api/models/LoginRequestDto";

export interface UserInfo {
    userId: string;
    authTokenExpiration: number;
    authToken: string;
    refreshToken: string;
}

export interface UserContextModel {
    userId: string | null;
    loggedIn: boolean;
    initialized: boolean;
    authError: string | null;
    handleLogin: (data: LoginRequestDto) => Promise<void>;
    handleLogout: () => Promise<void>;
}

export const UserContext = createContext<UserContextModel>({
    userId: null,
    loggedIn: false,
    initialized: false,
    authError: null,
    handleLogin: () => Promise.resolve(),
    handleLogout: () => Promise.resolve()
});

export function useUserContext() {
    const ctx = useContext(UserContext);
    if (!ctx) {
        throw new Error("useUserContext() must be used within UserContextProvider.");
    }
    
    return ctx;
}

