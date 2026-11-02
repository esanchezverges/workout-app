import { useQuery } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api, describeSet } from '../api'

export function ExerciseHistoryPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const exercises = useQuery({ queryKey: ['exercises'], queryFn: api.exercises })
  const history = useQuery({
    queryKey: ['exercise-history', id],
    queryFn: () => api.exerciseHistory(id!),
    enabled: id !== undefined,
  })

  return (
    <>
      <h1>Exercise history</h1>
      {exercises.isError && <p className="error">{exercises.error.message}</p>}
      <label>
        Exercise
        <select value={id ?? ''} onChange={(e) => navigate(`/exercises/${e.target.value}`)}>
          <option value="" disabled>Choose…</option>
          {exercises.data?.map((e) => <option key={e.id} value={e.id}>{e.name}</option>)}
        </select>
      </label>

      {history.isError && <p className="error">{history.error.message}</p>}
      {history.data?.length === 0 && <p>Nothing logged for this exercise yet.</p>}
      {history.data?.map((entry) => (
        <section className="card" key={entry.sessionId}>
          <h2><Link to={`/sessions/${entry.sessionId}`}>{entry.date}</Link></h2>
          <ol>
            {entry.sets.map((set) => <li key={set.id}>{describeSet(set)}</li>)}
          </ol>
        </section>
      ))}
    </>
  )
}
