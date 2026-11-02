import { NavLink, Route, Routes } from 'react-router-dom'
import { ExerciseHistoryPage } from './pages/ExerciseHistoryPage'
import { HistoryPage } from './pages/HistoryPage'
import { LogSessionPage } from './pages/LogSessionPage'
import { SessionPage } from './pages/SessionPage'

export default function App() {
  return (
    <>
      <header className="top">
        <strong>Workout Log</strong>
        <nav>
          <NavLink to="/" end>Log</NavLink>
          <NavLink to="/history">History</NavLink>
          <NavLink to="/exercises">Exercises</NavLink>
        </nav>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<LogSessionPage />} />
          <Route path="/history" element={<HistoryPage />} />
          <Route path="/sessions/:id" element={<SessionPage />} />
          <Route path="/exercises" element={<ExerciseHistoryPage />} />
          <Route path="/exercises/:id" element={<ExerciseHistoryPage />} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>
    </>
  )
}
