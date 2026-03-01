import checked from "../assets/checked.png";
import cancel from "../assets/cancel.png";
import easy from "../assets/easy.png";
import medium from "../assets/medium.png";
import hard from "../assets/hard.png";
import all from "../assets/all.png";

export default function SearchBar({
  query,
  onQueryChange,
  sortDirection,
  onToggleSort,
  difficultyFilter,
  onToggleDifficulty,
  solvedFilter,
  onToggleSolvedFilter,
}) {
  const solvedIcon = solvedFilter === "Unsolved" ? cancel : checked;
  const difficultyMap = {
    All: all,
    Easy: easy,
    Medium: medium,
    Hard: hard,
  };
  return (
    <div className="search-container">
      <form className="search-form" onSubmit={(e) => e.preventDefault()}>
        <label>
          <input
            className="search-bar"
            type="text"
            name="search-bar"
            value={query}
            onChange={(e) => onQueryChange(e.target.value)}
            placeholder="Search by question title, topic, description.."
          ></input>
        </label>
        <label>
          Sort
          <button
            name="sort-questions"
            className="sort-button"
            type="button"
            onClick={onToggleSort}
            title={`Sort by title (${sortDirection === "asc" ? "ascending" : "descending"})`}
          >
            <i
              className={
                sortDirection === "asc"
                  ? "bi bi-sort-alpha-down"
                  : "bi bi-sort-alpha-down-alt"
              }
            ></i>
          </button>
        </label>
        <label>
          Difficulty ({difficultyFilter})
          <button
            name="filter-difficulty"
            className="difficulty-button"
            type="button"
            onClick={onToggleDifficulty}
            title={`Current filter: ${difficultyFilter}`}
          >
            <img src={difficultyMap[difficultyFilter]}></img>
          </button>
        </label>
        <label>
          Solved ({solvedFilter})
          <button
            name="solved"
            className="filter-solved"
            type="button"
            onClick={onToggleSolvedFilter}
            title={`Current filter: ${solvedFilter}`}
          >
            <img src={solvedIcon} alt={solvedFilter}></img>
          </button>
        </label>
      </form>
    </div>
  );
}
