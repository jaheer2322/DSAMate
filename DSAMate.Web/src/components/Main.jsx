import QuestionList from "./QuestionList";
import Greet from "./Greet";
import UserProfilePanel from "./UserProfilePanel";

export default function Main() {
  return (
    <div className="main-container">
      <div className="main">
        <Greet />
        <QuestionList />
      </div>
      <div className="side-panel">
        <UserProfilePanel />
      </div>
    </div>
  );
}
