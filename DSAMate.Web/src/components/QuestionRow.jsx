// public Guid Id { get; set; }
// public string Title { get; set; }
// public string Description { get; set; }
// public string Difficulty { get; set; }
// public string Topic { get; set; }
// public string Hint { get; set; }
// public bool Solved { get; set; }
// public DateTime? SolvedAt { get; set; }

import { useState } from "react";
import checked from "../assets/checked.png";
import cancel from "../assets/cancel.png";

export default function QuestionRow({
  id,
  title,
  topic,
  difficulty,
  solved,
  reveal,
  toggleTopicReveal,
}) {
  const [isSolved, setIsSolved] = useState(solved);
  function handleSolve() {
    setIsSolved(!isSolved);
    solved = isSolved;
  }
  return (
    <tr>
      <td>{title}</td>
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
      <td style={{ "text-align": "center" }}>
        <span className={"diff " + difficulty}>{difficulty}</span>
      </td>
      <td style={{ "text-align": "center" }}>
        <button onClick={handleSolve}>
          <img src={isSolved ? checked : cancel}></img>
        </button>
      </td>
    </tr>
  );
}
