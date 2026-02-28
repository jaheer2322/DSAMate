import { createContext, useContext, useState } from "react";
import * as apiClient from "./apiClient";
import { jwtDecode } from "jwt-decode";

const AuthContext = createContext(undefined);

export default function AuthProvider({ children }) {
  const [token, setToken] = useState(
    localStorage.getItem("jwt_token_storage_key") ?? "",
  );
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const [isRegistering, setIsRegistering] = useState(false);
  const [userName, setUserName] = useState(
    localStorage.getItem("username_storage_key") ?? "",
  );
  function logout() {
    localStorage.removeItem("jwt_token_storage_key");
    localStorage.removeItem("username_storage_key");
    setToken("");
    setUserName("");
  }
  async function login({ emailAddress, password }) {
    setIsLoggingIn(true);
    try {
      const response = await apiClient.post("/auth/login", {
        emailAddress,
        password,
      });

      const jwtToken = response?.data?.jwtToken;
      if (!jwtToken) {
        throw new Error("Login did not return a jwt token!");
      }
      localStorage.setItem("jwt_token_storage_key", jwtToken);

      let decodedUsername = jwtDecode(jwtToken).Username;
      decodedUsername =
        decodedUsername.charAt(0).toUpperCase() + decodedUsername.slice(1);

      localStorage.setItem("username_storage_key", decodedUsername);

      setToken(jwtToken);
      setUserName(decodedUsername);
    } finally {
      setIsLoggingIn(false);
    }
  }
  async function register({ emailAddress, userName, password }) {
    setIsRegistering(true);
    let response;
    try {
      response = await apiClient.post("/auth/register", {
        emailAddress,
        userName,
        password,
        roles: ["user"],
      });
    } finally {
      setIsRegistering(false);
    }
    return response?.data;
  }
  const value = {
    token,
    userName,
    isAuthenticated: Boolean(token),
    isLoggingIn,
    register,
    isRegistering,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
