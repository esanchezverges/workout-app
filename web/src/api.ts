export type SetType = 'Weighted' | 'Timed'

export interface Exercise {
  id: number
  name: string
  description: string | null
  bodyPart: string
  equipment: string
  targetMuscle: string
  imageUrl: string | null
  videoUrl: string | null
  setType: SetType
}

export interface LoggedSet {
  id: number
  exerciseId: number
  exerciseName: string
  setNumber: number
  setType: SetType
  weight: number | null
  reps: number | null
  durationSeconds: number | null
  restSeconds: number | null
  notes: string | null
}

export interface Session {
  id: number
  date: string
  notes: string | null
  sets: LoggedSet[]
}

export interface SessionSummary {
  id: number
  date: string
  notes: string | null
  setCount: number
  exercises: string[]
}

export interface ExerciseHistoryEntry {
  sessionId: number
  date: string
  sets: LoggedSet[]
}

export interface NewSet {
  exerciseId: number
  weight: number | null
  reps: number | null
  durationSeconds: number | null
  restSeconds: number | null
  notes: string | null
}

export interface NewSession {
  date: string
  notes: string | null
  sets: NewSet[]
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, init)
  if (response.ok) return response.json() as Promise<T>

  const problem = await response.json().catch(() => null)
  const details = problem?.errors ? Object.values(problem.errors).flat().join(' ') : null
  throw new Error(details || problem?.title || `Request failed (${response.status})`)
}

export const api = {
  exercises: () => request<Exercise[]>('/api/exercises'),
  exerciseHistory: (id: string) => request<ExerciseHistoryEntry[]>(`/api/exercises/${id}/history`),
  sessions: () => request<SessionSummary[]>('/api/sessions'),
  session: (id: string) => request<Session>(`/api/sessions/${id}`),
  createSession: (session: NewSession) =>
    request<Session>('/api/sessions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(session),
    }),
}

export function describeSet(set: LoggedSet): string {
  const work = set.setType === 'Weighted' ? `${set.weight} kg × ${set.reps}` : `${set.durationSeconds}s`
  return set.restSeconds === null ? work : `${work}, ${set.restSeconds}s rest`
}
