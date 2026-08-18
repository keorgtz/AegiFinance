"use client";

import React, { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { authApi } from "@/lib/api/auth";
import { setAccessToken } from "@/lib/api/client";
import type { UserDto } from "@/types/api";

interface AuthState {
  user: UserDto | null;
  isLoading: boolean;
  isAuthenticated: boolean;
}

interface AuthContextValue extends AuthState {
  login: (usernameOrEmail: string, password: string) => Promise<void>;
  loginWithPin: (userName: string, pin: string) => Promise<void>;
  logout: () => Promise<void>;
  changePassword: (currentPassword: string, newPassword: string) => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [state, setState] = useState<AuthState>({
    user: null,
    isLoading: true,
    isAuthenticated: false,
  });
  const bootstrapped = useRef(false);

  const setUser = useCallback((user: UserDto | null) => {
    setState({ user, isLoading: false, isAuthenticated: !!user });
  }, []);

  // Intenta restaurar sesión al montar usando el refresh token en cookie
  useEffect(() => {
    if (bootstrapped.current) return;
    bootstrapped.current = true;

    (async () => {
      try {
        const res = await fetch("/api/auth/refresh", {
          method: "POST",
          credentials: "include",
        });

        if (res.ok) {
          const data = await res.json();
          setAccessToken(data.accessToken);
          setUser(data.user);
        } else {
          setUser(null);
        }
      } catch {
        setUser(null);
      }
    })();
  }, [setUser]);

  // Escucha el evento de unauthorized para hacer logout automático
  useEffect(() => {
    const handler = () => {
      setUser(null);
      router.push("/login");
    };

    window.addEventListener("aegifinance:unauthorized", handler);
    return () => window.removeEventListener("aegifinance:unauthorized", handler);
  }, [router, setUser]);

  const login = useCallback(async (usernameOrEmail: string, password: string) => {
    const data = await authApi.login(usernameOrEmail, password);
    setAccessToken(data.accessToken);
    setUser(data.user);
  }, [setUser]);

  const loginWithPin = useCallback(async (userName: string, pin: string) => {
    const data = await authApi.loginWithPin(userName, pin);
    setAccessToken(data.accessToken);
    setUser(data.user);
  }, [setUser]);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      setAccessToken(null);
      setUser(null);
      router.push("/login");
    }
  }, [router, setUser]);

  const changePassword = useCallback(async (currentPassword: string, newPassword: string) => {
    await authApi.changePassword(currentPassword, newPassword);
    setAccessToken(null);
    setUser(null);
    router.push("/login");
  }, [router, setUser]);

  return (
    <AuthContext.Provider value={{ ...state, login, loginWithPin, logout, changePassword }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth debe usarse dentro de <AuthProvider>");
  return ctx;
}
