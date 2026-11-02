import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api'

export function HistoryPage() {
  const sessions = useQuery({ queryKey: ['sessions'], queryFn: api.sessions })

  if (sessions.isPending) return <p>Loading sessions…</p>
  if (sessions.isError) return <p className="error">{sessions.error.message}</p>

  return (
    <>
      <h1>History</h1>
      {sessions.data.length === 0 && <p>No sessions yet. <Link to="/">Log your first one.</Link></p>}
      {sessions.data.map((session) => (
        <Link className="card row-link" key={session.id} to={`/sessions/${session.id}`}>
          <strong>{session.date}</strong>
          <span>{session.exercises.join(', ')}</span>
          <small>{session.setCount} {session.setCount === 1 ? 'set' : 'sets'}</small>
        </Link>
      ))}
    </>
  )
}
