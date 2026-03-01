import { useEffect, useState } from "react";
import * as apiClient from "../apiClient";
import { useAuth } from "../context";

function Chart({ progress }) {
  const topics = Object.keys(progress);
  return (
    <div className="chart-wrapper">
      <div className="profile-legend">
        <span>
          <i className="legend-box legend-total" /> Total Questions
        </span>
        <span>
          <i className="legend-box legend-solved" /> Questions Solved
        </span>
      </div>
      {topics.map((t) => (
        <>
          <p className="topic-heading">{t}</p>
          <div key={t} className="topic-card">
            <div className="progress">
              {progress[t].solved > 0 && (
                <div
                  className="completed"
                  style={{
                    width: `${(progress[t].solved / progress[t].total) * 100}%`,
                  }}
                >
                  {progress[t].solved}
                </div>
              )}
              <p className="total">{progress[t].total}</p>
            </div>
          </div>
        </>
      ))}
    </div>
  );
}

export default function UserProfilePanel() {
  const [progress, setProgress] = useState({});
  const [error, setError] = useState("");

  async function fetchProgress() {
    try {
      // setLoading(true);
      const response = await apiClient.get("/questions/progress");
      setProgress(response.data ?? {});
      setError("");
    } catch {
      setError("Could not fetch progress.");
    } finally {
      // setLoading(false);
    }
  }

  useEffect(() => {
    fetchProgress();
    function handleProgressChanged() {
      fetchProgress();
    }
    window.addEventListener("dsamate-progress-reset", handleProgressChanged);
    window.addEventListener("dsamate-question-updated", handleProgressChanged);

    return () => {
      window.removeEventListener(
        "dsamate-progress-reset",
        handleProgressChanged,
      );
      window.removeEventListener(
        "dsamate-question-updated",
        handleProgressChanged,
      );
    };
  }, []);
  const { userName } = useAuth();
  return (
    <div className="profile-overlay">
      <div className="profile-panel">
        <div className="profile-panel-header">
          <h3 className="progress-heading">{`${userName}'s`} Progress</h3>
        </div>
        <Chart progress={progress} />
        {error ? <p className="error-message">{error}</p> : ""}
      </div>
    </div>
  );
}
