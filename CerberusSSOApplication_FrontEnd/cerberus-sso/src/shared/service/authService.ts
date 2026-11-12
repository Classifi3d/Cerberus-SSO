import { apiClient } from "@/app/api/client";

export interface LoginCredentials {
  email: string;
  password: string;
  requestId?: string;
}

export type LoginResponse =
  { token: string | null } | { message: string; challengeId: string } | { redirectUrl: string };

export const login = async (credentials: LoginCredentials): Promise<LoginResponse> => {
  const { data } = await apiClient.post<LoginResponse>("/User/login", credentials);
  return data;
};
