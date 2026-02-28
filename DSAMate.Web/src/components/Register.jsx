import { useState } from "react";
import { useAuth } from "../context";
import { Link } from "react-router-dom";
import DsaMateLogo from "../assets/DsaMateLogo.png";

export default function Register() {
  const [creds, setCreds] = useState({
    emailAddress: "",
    userName: "",
    password: "",
  });
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const { register, isRegistering } = useAuth();
  function handleChange(e) {
    setCreds((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  }

  async function handleRegister(e) {
    e.preventDefault();
    setErrorMessage("");
    setSuccessMessage("");
    try {
      const response = await register(creds);
      setSuccessMessage(response.response);
    } catch (error) {
      const message = error?.response.data.response ?? "Unable to register";
      console.log(message);
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
        <h1>Create Account</h1>
        <form className="login-form" onSubmit={handleRegister}>
          <label>
            Email
            <input
              type="email"
              name="emailAddress"
              value={creds.emailAddress}
              onChange={handleChange}
              required
            />
          </label>

          <label>
            Username
            <input
              type="text"
              name="userName"
              value={creds.userName}
              onChange={handleChange}
              required
            />
          </label>

          <label>
            Password
            <input
              type="password"
              name="password"
              value={creds.password}
              onChange={handleChange}
              required
            />
          </label>

          {errorMessage ? (
            <p className="error-message">{errorMessage}</p>
          ) : null}
          {successMessage ? (
            <p className="success-message">{successMessage}</p>
          ) : null}

          <button type="submit" disabled={isRegistering}>
            {isRegistering ? "Registering..." : "Register"}
          </button>
          <div className="create-account-link">
            <span>Already have an account? </span>
            <Link to="/">Log in</Link>
          </div>
        </form>
      </section>
    </main>
  );
}
