import { useState } from "react";

export default function Login() {
  const [creds, setCreds] = useState({});

  function handleChange(e) {
    setCreds({ ...creds, [e.target.name]: e.target.value });
  }

  function handleLogin(e) {}

  return (
    <div className="login">
      <h1>Login</h1>
      <div>
        Email
        <input type="email" name="email" onChange={handleChange}></input>
      </div>
      <div>
        Password
        <input type="password" name="password" onChange={handleChange}></input>
      </div>
      <button onClick={handleLogin}>Login</button>
    </div>
  );
}
