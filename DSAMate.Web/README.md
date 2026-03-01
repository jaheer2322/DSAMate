# DSAMate Frontend (React + Vite)
A clean, interview-ready frontend for tracking DSA question practice.

## What this app demonstrates
- JWT-based login/register flow
- Protected app shell for authenticated users
- Search + filter + sort on question bank
- Per-question solved toggle with instant UI updates
- Topic-wise progress dashboard
- Random unsolved question recommendation

## Tech Stack
### 1) Authentication UX
- Login and registration screens with error handling
- JWT persisted in localStorage
- Username derived from token claims and reused across UI

### 2) Question Exploration
- Search by title/description/topic
- Filter by difficulty and solved status
- Sort by title
- Pagination (“load more” behavior)
- Topic reveal/hide controls for self-testing

### 3) Progress Tracking
- Side panel chart showing solved vs total per topic
- Auto-refreshes after solve/un-solve and reset events

### 4) Practice Assistance
- “Up-next” card to fetch a random unsolved question

## Frontend Architecture (high level)

- `src/context.jsx` → Auth provider (login/register/logout, token state)
- `src/apiClient.js` → HTTP client with auth headers + normalized error handling
- `src/components/QuestionList.jsx` → data fetch + filters + table state
- `src/components/UserProfilePanel.jsx` → progress API integration + chart rendering

## Local Setup

### Prerequisites
- Node.js 18+
- Backend API running on `https://localhost:7197`

### Install & run
```bash
npm install
npm run dev
```

### Build for production
```bash
npm run build
npm run preview
```

## API dependency

This frontend expects the backend base URL configured in:
- `src/apiClient.js` → `https://localhost:7197/api`
