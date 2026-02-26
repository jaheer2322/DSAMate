import { useState } from "react";
import Header from "./components/Header";
import Main from "./components/Main";
import Footer from "./components/Footer";
import Login from "./components/Login";

function App() {
  return (
    <>
      <Header />
      {/* <Login /> */}
      <Main />
      <Footer />
    </>
  );
}

export default App;
