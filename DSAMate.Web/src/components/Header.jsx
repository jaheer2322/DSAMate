import DsaMateLogo from "../assets/DsaMateLogo.png";
const navLinks = ["Home", "Progress"];
export default function Header() {
  return (
    <header>
      <div className="logo">
        <img className="logo-image" src={DsaMateLogo} alt="DsaMate Logo" />
        <span className="logo-text">DsaMate</span>
      </div>
      <div className="nav-links">
        {navLinks.map((nL, idx) => (
          <button className="nav-link-item" key={idx}>
            <a href="#">{nL}</a>
          </button>
        ))}
      </div>
    </header>
  );
}
