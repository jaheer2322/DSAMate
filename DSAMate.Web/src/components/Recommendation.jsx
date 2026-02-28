export default function Recommendation({ fetchRandom }) {
  return (
    <div class="recommendation">
      <div class="recommendation-text">
        <h2>Up-next:</h2>
      </div>
      <div class="cards">
        <div class="recommendation-text">
          <h3>Pick unsolved random question:</h3>
        </div>
        <div class="card">
          <button className="random" onClick={fetchRandom}>
            <h2 class="bi bi-shuffle"></h2>
          </button>
        </div>
      </div>
    </div>
  );
}
