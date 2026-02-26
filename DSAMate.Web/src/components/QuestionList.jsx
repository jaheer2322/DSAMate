import { useState } from "react";
import QuestionRow from "./QuestionRow";
import SearchBar from "./SearchBar";

let questionsList = [
  {
    id: "1f6d2e9a-1c3b-4a5a-9d2e-111111111111",
    title: "Two Sum",
    Description:
      "Given an array of integers and a target value, return indices of the two numbers such that they add up to the target.",
    difficulty: "Easy",
    topic: "Array",
    Hint: "Use a hash map to store visited numbers and their indices.",
    solved: true,
    solvedAt: "2026-01-15T10:30:00Z",
  },
  {
    id: "2a8c4c1e-9e4b-4d6c-8f3a-222222222222",
    title: "Valid Parentheses",
    Description: "Check if the input string containing brackets is valid.",
    difficulty: "Easy",
    topic: "Stack",
    Hint: "Push opening brackets onto a stack and match them with closing ones.",
    solved: true,
    solvedAt: "2026-01-16T09:15:00Z",
  },
  {
    id: "3b7e2f6d-3a9e-4e1b-b4c8-333333333333",
    title: "Best Time to Buy and Sell Stock",
    Description:
      "Find the maximum profit you can achieve from buying and selling a stock once.",
    difficulty: "Easy",
    topic: "Array",
    Hint: "Track the minimum price so far while iterating.",
    solved: false,
    solvedAt: null,
  },
  {
    id: "4c9a1d2e-5f6b-4a2c-9b1d-444444444444",
    title: "Longest Substring Without Repeating Characters",
    Description:
      "Find the length of the longest substring without repeating characters.",
    difficulty: "Medium",
    topic: "Sliding Window",
    Hint: "Use two pointers and a set or map to track characters.",
    solved: true,
    solvedAt: "2026-01-18T14:45:00Z",
  },
  {
    id: "5d3e8a2b-7c4f-4e9a-a1d6-555555555555",
    title: "Binary Search",
    Description:
      "Search for a target value in a sorted array using binary search.",
    difficulty: "Easy",
    topic: "Binary Search",
    Hint: "Repeatedly divide the search space in half.",
    solved: true,
    solvedAt: "2026-01-19T11:20:00Z",
  },
  {
    id: "6e4b9c2a-1d7f-4b6e-8a3c-666666666666",
    title: "Merge Two Sorted Lists",
    Description: "Merge two sorted linked lists and return the merged list.",
    difficulty: "Easy",
    topic: "Linked List",
    Hint: "Use a dummy node to simplify pointer handling.",
    solved: false,
    solvedAt: null,
  },
  {
    id: "7f1d8b4c-3a2e-4d9f-b6c1-777777777777",
    title: "Maximum Subarray",
    Description: "Find the contiguous subarray with the largest sum.",
    difficulty: "Medium",
    topic: "Dynamic Programming",
    Hint: "Use Kadane’s algorithm.",
    solved: true,
    solvedAt: "2026-01-22T16:00:00Z",
  },
  {
    id: "8a2c7d9e-6b4f-4e1a-9c8d-888888888888",
    title: "Invert Binary Tree",
    Description: "Invert a binary tree by swapping left and right children.",
    difficulty: "Easy",
    topic: "Tree",
    Hint: "Use recursion or BFS/DFS traversal.",
    solved: false,
    solvedAt: null,
  },
  {
    id: "9b6e1a4d-2f8c-4c9b-8e7a-999999999999",
    title: "Detect Cycle in Linked List",
    Description: "Determine if a linked list has a cycle.",
    difficulty: "Medium",
    topic: "Linked List",
    Hint: "Use slow and fast pointers (Floyd’s cycle detection).",
    solved: true,
    solvedAt: "2026-01-25T13:10:00Z",
  },
  {
    id: "a1c9d4e7-8b2f-4a6e-9d5b-aaaaaaaaaaaa",
    title: "Median of Two Sorted Arrays",
    Description: "Find the median of two sorted arrays in logarithmic time.",
    difficulty: "Hard",
    topic: "Binary Search",
    Hint: "Apply binary search on the smaller array to partition correctly.",
    solved: false,
    solvedAt: null,
  },
];

export default function QuestionList() {
  const [revealAll, setRevealAll] = useState(false);
  const initialQuestions = questionsList.map((q) => {
    return { ...q, reveal: revealAll };
  });
  const [questions, setQuestions] = useState(initialQuestions);
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
  return (
    <>
      <SearchBar />
      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Title</th>
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
              />
            ))}
          </tbody>
        </table>
      </div>
      <button className="load-more">Load more</button>
    </>
  );
}
