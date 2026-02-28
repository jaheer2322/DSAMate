import { useEffect, useMemo, useState } from "react";
import * as apiClient from "../apiClient";
import { useAuth } from "../context";

function RadarChart({ progress }) {
  const topics = useMemo(() => Object.keys(progress ?? {}), [progress]);

  if (!topics.length) {
    return <p className="profile-empty">No topic progress found yet.</p>;
  }

  const maxTotal = Math.max(
    ...topics.map(
      (topic) => progress[topic]?.total ?? progress[topic]?.Total ?? 0,
    ),
    1,
  );

  const size = 260;
  const center = size / 2;
  const radius = 90;

  const getPoint = (index, value) => {
    const angle = (Math.PI * 2 * index) / topics.length - Math.PI / 2;
    const scaled = (value / maxTotal) * radius;
    return {
      x: center + Math.cos(angle) * scaled,
      y: center + Math.sin(angle) * scaled,
    };
  };

  const totalPoints = topics
    .map((topic, i) => {
      const totalValue = progress[topic]?.total ?? progress[topic]?.Total ?? 0;
      const point = getPoint(i, totalValue);
      return `${point.x},${point.y}`;
    })
    .join(" ");

  const solvedPoints = topics
    .map((topic, i) => {
      const solvedValue =
        progress[topic]?.solved ?? progress[topic]?.Solved ?? 0;
      const point = getPoint(i, solvedValue);
      return `${point.x},${point.y}`;
    })
    .join(" ");

  return (
    <div className="radar-wrapper">
      <svg viewBox={`0 0 ${size} ${size}`} className="profile-radar-chart">
        {[0.25, 0.5, 0.75, 1].map((step) => (
          <circle
            key={step}
            cx={center}
            cy={center}
            r={radius * step}
            fill="none"
            stroke="#5d6a73"
            strokeDasharray="4 4"
          />
        ))}

        {topics.map((topic, i) => {
          const axisPoint = getPoint(i, maxTotal);
          return (
            <g key={topic}>
              <line
                x1={center}
                y1={center}
                x2={axisPoint.x}
                y2={axisPoint.y}
                stroke="#5d6a73"
              />
              <text
                x={center + (axisPoint.x - center) * 1.12}
                y={center + (axisPoint.y - center) * 1.12}
                fill="#dce6ee"
                fontSize="10"
                textAnchor="middle"
                dominantBaseline="middle"
              >
                {topic === "DynamicProgramming" ? "DP" : topic}
              </text>
            </g>
          );
        })}

        <polygon
          points={totalPoints}
          fill="rgba(255, 155, 81, 0.28)"
          stroke="#ff9b51"
          strokeWidth="2"
        />
        <polygon
          points={solvedPoints}
          fill="rgba(56, 165, 120, 0.35)"
          stroke="#38a578"
          strokeWidth="2"
        />
      </svg>
      <div className="profile-legend">
        <span>
          <i className="legend-box legend-total" /> Total Questions
        </span>
        <span>
          <i className="legend-box legend-solved" /> Questions Solved
        </span>
      </div>
    </div>
  );
}

export default function UserProfilePanel({ onClose }) {
  const [progress, setProgress] = useState({});
  const [loading, setLoading] = useState(true);
  const [isResetting, setIsResetting] = useState(false);
  const [error, setError] = useState("");

  async function fetchProgress() {
    try {
      setLoading(true);
      const response = await apiClient.get("/questions/progress");
      setProgress(response.data ?? {});
      setError("");
    } catch {
      setError("Could not fetch progress.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchProgress();
  }, []);

  async function handleResetProgress() {
    try {
      setIsResetting(true);
      await apiClient.post("/questions/reset-progress", {});
      window.dispatchEvent(new Event("dsamate-progress-reset"));
      await fetchProgress();
    } catch {
      setError("Could not reset progress.");
    } finally {
      setIsResetting(false);
    }
  }
  const { userName } = useAuth();
  return (
    <div className="profile-overlay" onClick={onClose}>
      <div
        className="profile-panel"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="profile-panel-header">
          <h3>{`${userName}'s`} Progress</h3>
        </div>
        {loading ? (
          <p>Loading progress...</p>
        ) : (
          <RadarChart progress={progress} />
        )}
        {error ? <p className="error-message">{error}</p> : ""}
        <button
          className="reset-progress-button"
          onClick={handleResetProgress}
          disabled={isResetting}
        >
          {isResetting ? "Resetting..." : "Reset Progress"}
        </button>
      </div>
    </div>
  );
}
