import DsaMateLogo from "../assets/DsaMateLogo.png";
import { useAuth } from "../context";
export default function Header() {
  const { userName, logout } = useAuth();
  return (
    <header>
      <div className="logo">
        <img className="logo-image" src={DsaMateLogo} alt="DsaMate Logo" />
        <span className="logo-text">DsaMate</span>
      </div>
      <div className="nav-links">
        <button className="nav-link-item">
          <a href="#">Home</a>
        </button>
        <button className="nav-link-item" onClick={logout}>
          Logout
        </button>
        <button className="nav-link-item">
          <div className="user-profile">
            <span>{userName.charAt(0)}</span>
          </div>
        </button>
      </div>
    </header>
  );
}
