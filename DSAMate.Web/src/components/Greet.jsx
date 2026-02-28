import { useAuth } from "../context";

const Greet = () => {
  const { userName } = useAuth();
  return (
    <div
      style={{
        color: "#c2c2c2",
        "font-size": "1.2em",
        "font-family": "bitcount single",
      }}
    >
      Hi <span>{userName}</span> welcome back!
    </div>
  );
};

export default Greet;
