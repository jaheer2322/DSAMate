import QuestionList from "./QuestionList";
import UserProfilePanel from "./UserProfilePanel";

export default function Main() {
  return (
    <div className="main-container">
      <div className="main">
        <QuestionList />
      </div>
      <div className="side-panel">
        <UserProfilePanel />
      </div>
    </div>
  );
}
