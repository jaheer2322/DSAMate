import checked from "../assets/checked.png";
import cancel from "../assets/cancel.png";

export default function QuestionRow({
  id,
  title,
  description,
  topic,
  difficulty,
  solved,
  reveal,
  toggleTopicReveal,
  onSolvedToggle,
}) {
  return (
    <tr>
      <td className="question-title">
        {title}
        <span className="question-description-tooltip">{description}</span>
      </td>
      <td className="topic-text">
        {!reveal && (
          <button
            onClick={() => toggleTopicReveal(id)}
            className="topic-reveal-button"
          >
            <i className={reveal ? "bi bi-eye-slash" : "bi bi-eye"}></i>
          </button>
        )}
        {reveal && topic}
      </td>
      <td style={{ textAlign: "center" }}>
        <span className={"diff " + difficulty}>{difficulty}</span>
      </td>
      <td style={{ textAlign: "center" }}>
        <button onClick={() => onSolvedToggle(id)}>
          <img
            src={solved ? checked : cancel}
            alt={solved ? "Solved" : "Unsolved"}
          ></img>
        </button>
      </td>
    </tr>
  );
}
