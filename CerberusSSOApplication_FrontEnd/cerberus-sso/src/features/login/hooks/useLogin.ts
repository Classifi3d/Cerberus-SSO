import { useMutation } from "@tanstack/react-query";
import type { AxiosError } from "axios";

import { type LoginCredentials, type LoginResponse, login } from "@/shared/service/authService";

export const useLogin = () =>
  useMutation<LoginResponse, AxiosError, LoginCredentials>({ mutationFn: login });
