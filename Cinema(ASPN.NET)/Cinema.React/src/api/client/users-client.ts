import { get, postAsJson, postAsJsonWithoutResponse } from "@/api/client/http";
import { LoginResponseDto } from "@/api/models/LoginResponseDto";
import { LoginRequestDto } from "@/api/models/LoginRequestDto";
import { UserRequestDto } from "@/api/models/UserRequestDto";
import { UserResponseDto } from "@/api/models/UserResponseDto";

export async function login(loginDto: LoginRequestDto): Promise<LoginResponseDto> {
    return await postAsJson<LoginRequestDto, LoginResponseDto>("users/login", loginDto);
}

export async function logout(): Promise<void> {
    await postAsJsonWithoutResponse("users/logout");
}

export async function refresh(refreshToken: string): Promise<LoginResponseDto> {
   return await postAsJson<string, LoginResponseDto>("users/refresh", refreshToken);
}

export async function createUser(data: UserRequestDto): Promise<UserResponseDto> {
    return await postAsJson<UserRequestDto, UserResponseDto>("users", data);
}

export async function getUserById(id: string): Promise<UserResponseDto> {
    return get<UserResponseDto>(`users/${id}`);
}