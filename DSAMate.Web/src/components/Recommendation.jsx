export default function Recommendation() {
  return (
    <div class="recommendation">
      <div class="recommendation-text">
        <h2>Up-next:</h2>
      </div>
      <div class="cards">
        <div class="recommendation-text">
          <h3>Based on your progress:</h3>
        </div>
        <div class="card">
          <a href="#">
            <h2>Two Sum</h2>
          </a>
          <p>Easy</p>
        </div>
        <div class="recommendation-text">
          <h3>Pick a random:</h3>
        </div>
        <div class="card">
          <button className="random" onClick={() => console.log("clicked")}>
            <h2 class="bi bi-shuffle"></h2>
          </button>
        </div>
      </div>
    </div>
  );
}
