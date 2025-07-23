import { UserInfo } from "@/contexts/UserContext";

/**
 * A reusable access token factory for SignalR HubConnections.
 */
export async function accessTokenFactory(): Promise<string | undefined> {
    const userSessionItem = localStorage.getItem("user");
    if (!userSessionItem) {
        return undefined;
    }

    const userFromSession = JSON.parse(userSessionItem) as UserInfo;
    return userFromSession.authToken;
}