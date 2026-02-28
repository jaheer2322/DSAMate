import { useState } from "react";
import * as apiClient from "../apiClient";

export default function ResetProgressDialog({ onClose }) {
  const [isResetting, setIsResetting] = useState(false);
  const [error, setError] = useState("");

  async function handleResetProgress() {
    try {
      setIsResetting(true);
      await apiClient.post("/questions/reset-progress", {});
      window.dispatchEvent(new Event("dsamate-progress-reset"));
      onClose();
    } catch {
      setError("Could not reset progress.");
    } finally {
      setIsResetting(false);
    }
  }

  return (
    <div className="profile-menu" role="dialog" aria-label="User profile menu">
      <p className="profile-menu-title">Progress actions</p>
      <button
        className="reset-progress-button"
        onClick={handleResetProgress}
        disabled={isResetting}
      >
        {isResetting ? "Resetting..." : "Reset Progress"}
      </button>
      {error ? <p className="error-message">{error}</p> : ""}
    </div>
  );
}
