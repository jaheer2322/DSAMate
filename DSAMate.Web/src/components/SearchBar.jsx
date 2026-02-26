import checked from "../assets/checked.png";
import cancel from "../assets/cancel.png";

export default function SearchBar() {
  return (
    <div className="search-container">
      <form className="search-form">
        <label>
          <input className="search-bar" type="text" name="search-bar"></input>
          <button className="search-button">
            <i class="bi bi-search"></i>
          </button>
        </label>
        <label>
          Sort
          <button name="sort-questions" className="sort-button">
            <i class="bi bi-sort-alpha-down"></i>
          </button>
        </label>
        <label>
          Difficulty
          <button name="filter-difficulty" className="difficulty-button">
            <i class="bi bi-sort-down"></i>
          </button>
        </label>
        <label>
          Solved
          <button name="solved" className="filter-solved">
            <img src={checked}></img>
          </button>
        </label>
      </form>
    </div>
  );
}
