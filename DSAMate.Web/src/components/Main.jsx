import QuestionList from "./QuestionList";
import Recommendation from "./Recommendation";
import Greet from "./Greet";

export default function Main() {
  return (
    <div className="main">
      <Greet />
      <Recommendation />
      <QuestionList />
    </div>
  );
}
