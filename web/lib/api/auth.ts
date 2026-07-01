import { api } from "./client";
import type { AuthResponseDto, UserDto } from "@/types/api";

export const authApi = {
  login(usernameOrEmail: string, password: string) {
    return api.post<AuthResponseDto>("/auth/login", { usernameOrEmail, password });
  },

  loginWithPin(userName: string, pin: string) {
    return api.post<AuthResponseDto>("/auth/login-pin", { userName, pin });
  },

  logout() {
    return api.post<void>("/auth/logout");
  },

  me() {
    return api.get<UserDto>("/auth/me");
  },

  changePassword(currentPassword: string, newPassword: string) {
    return api.post<void>("/auth/change-password", { currentPassword, newPassword });
  },
};
