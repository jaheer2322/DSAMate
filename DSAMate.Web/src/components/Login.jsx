import { useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../context";
import DsaMateLogo from "../assets/DsaMateLogo.png";

export default function Login() {
  const [creds, setCreds] = useState({ emailAddress: "", password: "" });
  const [errorMessage, setErrorMessage] = useState("");
  const { login, isLoggingIn } = useAuth();

  function handleChange(e) {
    setCreds((prevCreds) => ({
      ...prevCreds,
      [e.target.name]: e.target.value,
    }));
  }

  async function handleLogin(e) {
    e.preventDefault();
    setErrorMessage("");
    try {
      await login(creds);
    } catch (error) {
      const message = error?.response?.data.response ?? "Unable to login.";
      setErrorMessage(message);
    }
  }

  return (
    <main className="login-page-container">
      <div className="logo">
        <img className="logo-image" src={DsaMateLogo} alt="DsaMate Logo" />
        <span className="logo-text" style={{ color: "white" }}>
          DsaMate
        </span>
      </div>
      <section className="login-card">
        <h1>Login</h1>
        <form className="login-form" onSubmit={handleLogin}>
          <label>
            Email
            <input
              type="email"
              name="emailAddress"
              onChange={handleChange}
              value={creds.emailAddress}
              required
            ></input>
          </label>

          <label>
            Password
            <input
              type="password"
              name="password"
              onChange={handleChange}
              value={creds.password}
              required
            ></input>
          </label>

          {errorMessage ? (
            <p className="error-message">{errorMessage}</p>
          ) : null}

          <button type="submit" disabled={isLoggingIn}>
            {isLoggingIn ? "Logging in..." : "Login"}
          </button>
          <div className="create-account-link">
            <span>Don't have an account? </span>
            <Link to="/register">Create account</Link>
          </div>
        </form>
      </section>
    </main>
  );
}
