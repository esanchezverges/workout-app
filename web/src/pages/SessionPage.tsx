import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { api, describeSet } from '../api'

export function SessionPage() {
  const { id = '' } = useParams()
  const session = useQuery({ queryKey: ['sessions', id], queryFn: () => api.session(id) })

  if (session.isPending) return <p>Loading session…</p>
  if (session.isError) return <p className="error">{session.error.message}</p>

  const exerciseIds = [...new Set(session.data.sets.map((set) => set.exerciseId))]

  return (
    <>
      <h1>{session.data.date}</h1>
      {session.data.notes && <p>{session.data.notes}</p>}
      {exerciseIds.map((exerciseId) => {
        const sets = session.data.sets.filter((set) => set.exerciseId === exerciseId)
        return (
          <section className="card" key={exerciseId}>
            <h2><Link to={`/exercises/${exerciseId}`}>{sets[0].exerciseName}</Link></h2>
            <ol>
              {sets.map((set) => <li key={set.id}>{describeSet(set)}</li>)}
            </ol>
          </section>
        )
      })}
    </>
  )
}
