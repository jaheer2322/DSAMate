import DsaMateLogo from "../assets/DsaMateLogo.png";
import { useState } from "react";
import { useAuth } from "../context";
export default function Header() {
  const { userName, logout } = useAuth();
  const [isProfileOpen, setIsProfileOpen] = useState(false);
  return (
    <>
      <header>
        <div className="logo">
          <img className="logo-image" src={DsaMateLogo} alt="DsaMate Logo" />
          <span className="logo-text">DsaMate</span>
        </div>
        <div className="nav-links">
          <button className="nav-link-item">
            <a href="#" onClick={() => location.reload()}>
              Home
            </a>
          </button>
          <button className="nav-link-item" onClick={logout}>
            Logout
          </button>
          <button
            className="nav-link-item"
            onClick={() => setIsProfileOpen((prevOpen) => !prevOpen)}
            aria-label="Open user profile"
          >
            <div className="user-profile">
              <span>{userName.charAt(0)}</span>
            </div>
          </button>
        </div>
      </header>
    </>
  );
}
