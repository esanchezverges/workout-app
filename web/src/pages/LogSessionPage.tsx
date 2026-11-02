import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, type Exercise, type NewSet } from '../api'

interface SetDraft {
  key: number
  weight: string
  reps: string
  durationSeconds: string
  restSeconds: string
}

interface ExerciseDraft {
  exercise: Exercise
  sets: SetDraft[]
}

let nextKey = 1
const blankSet = (from?: SetDraft): SetDraft => ({
  key: nextKey++,
  weight: from?.weight ?? '',
  reps: from?.reps ?? '',
  durationSeconds: from?.durationSeconds ?? '',
  restSeconds: from?.restSeconds ?? '',
})

const toNumber = (value: string) => (value === '' ? null : Number(value))

function toNewSets({ exercise, sets }: ExerciseDraft): NewSet[] {
  const weighted = exercise.setType === 'Weighted'
  return sets.map((set) => ({
    exerciseId: exercise.id,
    weight: weighted ? toNumber(set.weight) : null,
    reps: weighted ? toNumber(set.reps) : null,
    durationSeconds: weighted ? null : toNumber(set.durationSeconds),
    restSeconds: toNumber(set.restSeconds),
    notes: null,
  }))
}

export function LogSessionPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const exercises = useQuery({ queryKey: ['exercises'], queryFn: api.exercises })

  const [date, setDate] = useState(() => new Date().toLocaleDateString('en-CA'))
  const [notes, setNotes] = useState('')
  const [drafts, setDrafts] = useState<ExerciseDraft[]>([])

  const save = useMutation({
    mutationFn: api.createSession,
    onSuccess: async (session) => {
      await queryClient.invalidateQueries({ queryKey: ['sessions'] })
      await queryClient.invalidateQueries({ queryKey: ['exercise-history'] })
      navigate(`/sessions/${session.id}`)
    },
  })

  const available = exercises.data?.filter((e) => !drafts.some((d) => d.exercise.id === e.id)) ?? []

  const addExercise = (id: string) => {
    const exercise = exercises.data?.find((e) => e.id === Number(id))
    if (exercise) setDrafts([...drafts, { exercise, sets: [blankSet()] }])
  }

  const updateSets = (exerciseId: number, change: (sets: SetDraft[]) => SetDraft[]) =>
    setDrafts(
      drafts
        .map((d) => (d.exercise.id === exerciseId ? { ...d, sets: change(d.sets) } : d))
        .filter((d) => d.sets.length > 0),
    )

  const submit = (event: FormEvent) => {
    event.preventDefault()
    save.mutate({ date, notes: notes.trim() || null, sets: drafts.flatMap(toNewSets) })
  }

  if (exercises.isPending) return <p>Loading exercises…</p>
  if (exercises.isError) return <p className="error">{exercises.error.message}</p>

  return (
    <form onSubmit={submit}>
      <h1>Log a session</h1>
      <label>
        Date
        <input type="date" required value={date} onChange={(e) => setDate(e.target.value)} />
      </label>

      {drafts.map(({ exercise, sets }) => (
        <section className="card" key={exercise.id}>
          <h2>{exercise.name}</h2>
          {sets.map((set, index) => {
            const update = (field: keyof SetDraft, value: string) =>
              updateSets(exercise.id, (all) => all.map((s) => (s.key === set.key ? { ...s, [field]: value } : s)))
            return (
              <div className="set-row" key={set.key}>
                <span className="set-number">{index + 1}</span>
                {exercise.setType === 'Weighted' ? (
                  <>
                    <label>
                      kg
                      <input type="number" inputMode="decimal" min="0" step="0.25" required value={set.weight}
                        onChange={(e) => update('weight', e.target.value)} />
                    </label>
                    <label>
                      Reps
                      <input type="number" inputMode="numeric" min="1" required value={set.reps}
                        onChange={(e) => update('reps', e.target.value)} />
                    </label>
                  </>
                ) : (
                  <label>
                    Seconds
                    <input type="number" inputMode="numeric" min="1" required value={set.durationSeconds}
                      onChange={(e) => update('durationSeconds', e.target.value)} />
                  </label>
                )}
                <label>
                  Rest (s)
                  <input type="number" inputMode="numeric" min="0" value={set.restSeconds}
                    onChange={(e) => update('restSeconds', e.target.value)} />
                </label>
                <button type="button" className="quiet" aria-label={`Remove set ${index + 1}`}
                  onClick={() => updateSets(exercise.id, (all) => all.filter((s) => s.key !== set.key))}>
                  ×
                </button>
              </div>
            )
          })}
          <button type="button" className="quiet"
            onClick={() => updateSets(exercise.id, (all) => [...all, blankSet(all.at(-1))])}>
            Add set
          </button>
        </section>
      ))}

      <label>
        Add an exercise
        <select value="" onChange={(e) => addExercise(e.target.value)}>
          <option value="" disabled>Choose…</option>
          {available.map((e) => (
            <option key={e.id} value={e.id}>{e.name} ({e.bodyPart})</option>
          ))}
        </select>
      </label>

      <label>
        Notes
        <textarea maxLength={1000} rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
      </label>

      {save.isError && <p className="error" role="alert">{save.error.message}</p>}
      <button type="submit" disabled={drafts.length === 0 || save.isPending}>
        {save.isPending ? 'Saving…' : 'Save session'}
      </button>
    </form>
  )
}
