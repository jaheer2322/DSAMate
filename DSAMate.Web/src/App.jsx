import Header from "./components/Header";
import Main from "./components/Main";
import Footer from "./components/Footer";
import { useAuth } from "./context";
import Login from "./components/Login";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Register from "./components/Register";

function App() {
  const { isAuthenticated } = useAuth();
  if (!isAuthenticated) {
    return (
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Login />} />
          <Route path="/register" element={<Register />} />
        </Routes>
      </BrowserRouter>
    );
  }
  return (
    <>
      <Header />
      <Main />
      <Footer />
    </>
  );
}

export default App;
