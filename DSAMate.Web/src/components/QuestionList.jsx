import { useState, useEffect, useRef } from "react";
import QuestionRow from "./QuestionRow";
import SearchBar from "./SearchBar";
import Recommendation from "./Recommendation";
import * as apiClient from "../apiClient";

const difficultyFilters = ["All", "Easy", "Medium", "Hard"];
const solvedFilters = ["All", "Solved", "Unsolved"];

export default function QuestionList() {
  const [isLoading, setIsLoading] = useState(false);
  const [revealAll, setRevealAll] = useState(false);
  const [allQuestionsSolved, setAllQuestionsSolved] = useState(false);
  const [questions, setQuestions] = useState([]);
  const [moreToLoad, setMoreToLoad] = useState(true);
  const [query, setQuery] = useState("");
  const [sortDirection, setSortDirection] = useState("asc");
  const [difficultyFilter, setDifficultyFilter] = useState("All");
  const [solvedFilter, setSolvedFilter] = useState("All");
  const [errorMessage, setErrorMessage] = useState("");
  const [page, setPage] = useState(1);
  const abortRef = useRef(null);

  async function fetchQuestions() {
    setIsLoading(true);
    // Cancel previous request
    if (abortRef.current) {
      abortRef.current.abort();
    }
    const controller = new AbortController();
    abortRef.current = controller;

    try {
      const searchParams = new URLSearchParams();
      if (query.trim()) {
        searchParams.set("search", query.trim());
      }
      if (difficultyFilter !== "All") {
        searchParams.set("difficulty", difficultyFilter);
      }
      if (solvedFilter !== "All") {
        searchParams.set(
          "solved",
          solvedFilter === "Solved" ? "true" : "false",
        );
      }

      searchParams.set("sortBy", "title");
      searchParams.set("isAscending", String(sortDirection === "asc"));
      searchParams.set("pageNumber", `${page}`);
      searchParams.set("pageSize", "10");

      const path = `/questions?${searchParams.toString()}`;
      const response = await apiClient.get(path, controller.signal);
      const data = response.data ?? [];
      const newQuestions = data.map((q) => ({
        id: q.id ?? q.Id,
        title: q.title ?? q.Title,
        description: q.description ?? q.description,
        topic: q.topic ?? q.Topic,
        difficulty: q.difficulty ?? q.Difficulty,
        solved: q.solved ?? q.Solved,
        solvedAt: q.solvedAt ?? q.SolvedAt,
        reveal: revealAll,
      }));

      if (newQuestions.length == 0) setMoreToLoad(false);
      else setMoreToLoad(true);

      if (page === 1) {
        setQuestions(newQuestions);
      } else {
        setQuestions((prev) => [...prev, ...newQuestions]);
      }

      setErrorMessage("");
    } catch (error) {
      if (error.name !== "AbortError") {
        setErrorMessage("Could not fetch questions. Please ");
      }
    } finally {
      setIsLoading(false);
    }
  }
  useEffect(() => {
    return () => {
      if (abortRef.current) {
        abortRef.current.abort();
      }
    };
  }, []);
  async function fetchRandom() {
    try {
      const path = "/questions/random";
      const response = await apiClient.get(path);
      const data = response.data ?? [];
      if (data.length === 0) {
        setQuestions([]);
        setAllQuestionsSolved(true);
      } else {
        const randomQuestion = {
          id: data.id ?? data.Id,
          title: data.title ?? data.Title,
          description: data.description ?? data.description,
          topic: data.topic ?? data.Topic,
          difficulty: data.difficulty ?? data.Difficulty,
          solved: data.solved ?? data.Solved,
          solvedAt: data.solvedAt ?? data.SolvedAt,
          reveal: revealAll,
        };
        setQuestions([randomQuestion]);
        setAllQuestionsSolved(false);
      }
      setMoreToLoad(false);
    } catch (error) {
      if (error.name !== "AbortError") {
        setErrorMessage("Could not fetch questions. Please ");
      }
    }
  }

  async function markAsSolved(id) {
    try {
      await apiClient.post(`/questions/${id}/mark-solved`);
    } catch (error) {
      if (error.name !== "AbortError") {
        setErrorMessage("Could not update the status. Please ");
      }
    }
  }

  async function handleLoadMore() {
    setPage((prev) => prev + 1);
  }

  function handleTopicRevealAll() {
    setRevealAll((prevRevealAll) => {
      const currRevealAll = !prevRevealAll;
      setQuestions((prev) =>
        prev.map((q) => {
          return { ...q, reveal: currRevealAll };
        }),
      );
      return currRevealAll;
    });
  }

  function toggleTopicReveal(id) {
    setQuestions((prev) =>
      prev.map((q) => {
        return q.id === id ? { ...q, reveal: !q.reveal } : q;
      }),
    );
  }

  async function handleSolvedToggle(id) {
    await markAsSolved(id);
    if (errorMessage === "") {
      setQuestions((prev) =>
        prev.map((q) => {
          return q.id === id
            ? {
                ...q,
                solved: !q.solved,
                solvedAt: !q.solved ? new Date().toISOString() : null,
              }
            : q;
        }),
      );
      window.dispatchEvent(new Event("dsamate-question-updated"));
    }
  }

  function handleDifficultyFilter() {
    const index = difficultyFilters.indexOf(difficultyFilter);
    setDifficultyFilter(
      difficultyFilters[(index + 1) % difficultyFilters.length],
    );
    setPage(1);
  }

  function handleSolvedFilter() {
    const index = solvedFilters.indexOf(solvedFilter);
    setSolvedFilter(solvedFilters[(index + 1) % solvedFilters.length]);
    setPage(1);
  }

  function handleReset() {
    setQuery("");
    setSortDirection("asc");
    setDifficultyFilter("All");
    setSolvedFilter("All");
    setErrorMessage("");
    setMoreToLoad(true);
    setRevealAll(false);
    setQuestions([]);

    if (page === 1) fetchQuestions();
    else setPage(1);
  }

  useEffect(() => {
    fetchQuestions();
  }, [query, sortDirection, difficultyFilter, solvedFilter, page]);

  useEffect(() => {
    function handleProgressReset() {
      setAllQuestionsSolved(false);
      setMoreToLoad(true);
      if (page === 1) {
        fetchQuestions();
      } else {
        setPage(1);
      }
    }

    window.addEventListener("dsamate-progress-reset", handleProgressReset);
    return () => {
      window.removeEventListener("dsamate-progress-reset", handleProgressReset);
    };
  }, [page, query, sortDirection, difficultyFilter, solvedFilter]);

  return (
    <>
      <Recommendation fetchRandom={fetchRandom} />
      <SearchBar
        query={query}
        onQueryChange={setQuery}
        sortDirection={sortDirection}
        onToggleSort={() => {
          setSortDirection((prev) => (prev === "asc" ? "desc" : "asc"));
          setPage(1);
        }}
        difficultyFilter={difficultyFilter}
        onToggleDifficulty={handleDifficultyFilter}
        solvedFilter={solvedFilter}
        onToggleSolvedFilter={handleSolvedFilter}
      />
      {errorMessage && (
        <p className="error-message">
          {errorMessage}{" "}
          <a href="#" onClick={() => location.reload()}>
            reload
          </a>
        </p>
      )}

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>
                {isLoading ? (
                  <p className="loading-icon" style={{ margin: 0 }}>
                    Loading...
                  </p>
                ) : (
                  ""
                )}
                Title
              </th>
              <th className="topic-text">
                Topic
                <button
                  className="topic-reveal-button"
                  onClick={handleTopicRevealAll}
                >
                  <i
                    className={
                      "bi " + (revealAll ? "bi bi-eye-slash" : "bi bi-eye")
                    }
                  ></i>
                </button>
              </th>
              <th>Difficulty</th>
              <th>Solved</th>
            </tr>
          </thead>
          <tbody>
            {questions.map((q) => (
              <QuestionRow
                key={q.id}
                {...q}
                toggleTopicReveal={toggleTopicReveal}
                onSolvedToggle={handleSolvedToggle}
              />
            ))}
          </tbody>
        </table>
      </div>
      {allQuestionsSolved ? (
        <p style={{ color: "lightgreen  " }}>
          You have solved all the questions! Reset your progress from your
          profile to get an unsolved random question.
        </p>
      ) : (
        ""
      )}
      {moreToLoad ? (
        <button className="load-more" onClick={handleLoadMore}>
          Load more
        </button>
      ) : (
        <button className="load-more" onClick={handleReset}>
          Reset
        </button>
      )}
    </>
  );
}
