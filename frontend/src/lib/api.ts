export const API_BASE_URL: string =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5246'

export interface ApiStatus {
  service: string
  organisation: string
  zvr: string
  phase: number
  mongo: 'ok' | 'unreachable'
  timeUtc: string
}

export async function fetchStatus(): Promise<ApiStatus> {
  const res = await fetch(`${API_BASE_URL}/api/status`)
  if (!res.ok) throw new Error(`API responded ${res.status}`)
  return res.json()
}
