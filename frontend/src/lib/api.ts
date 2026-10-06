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

/** Fees are set by the General Assembly (§12(f)); 0 means not yet set. */
export interface PublicConfig {
  joiningFee: number
  annualFeeOrdinary: number
  annualFeeSupporting: number
  currency: string
  statutesVersion: string
  privacyPolicyVersion: string
}

export async function fetchPublicConfig(): Promise<PublicConfig> {
  const res = await fetch(`${API_BASE_URL}/api/config/public`)
  if (!res.ok) throw new Error(`API responded ${res.status}`)
  return res.json()
}

export type MemberType = 'Person' | 'Organisation'
export type ApplicableClass = 'Ordinary' | 'Supporting'

export interface ApplicationRequest {
  memberType: MemberType
  name: string
  representative?: string
  dateOfBirth?: string // yyyy-mm-dd
  email: string
  postalAddress?: string
  country?: string
  requestedClass: ApplicableClass
  motivation?: string
  sharesValues: boolean
  hasReadStatutes: boolean
  acceptsPrivacyPolicy: boolean
  statutesVersion: string
  privacyPolicyVersion: string
}

/** Field names as the API reports them (PascalCase), mapped to messages. */
export type FieldErrors = Record<string, string>

export type ApiResult =
  | { ok: true; message: string }
  | { ok: false; status: number; message: string; fieldErrors: FieldErrors }

interface ProblemDetails {
  title?: string
  errors?: Record<string, string[]>
}

async function post(path: string, body: unknown): Promise<ApiResult> {
  let res: Response
  try {
    res = await fetch(`${API_BASE_URL}${path}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })
  } catch {
    return { ok: false, status: 0, message: 'Could not reach the server. Please check your connection and try again.', fieldErrors: {} }
  }

  const data = (await res.json().catch(() => ({}))) as ProblemDetails & { message?: string }
  if (res.ok) return { ok: true, message: data.message ?? '' }

  const fieldErrors: FieldErrors = {}
  for (const [field, messages] of Object.entries(data.errors ?? {})) {
    // Model-binding errors arrive as "$.field" or "Field"; normalise to the property name.
    const key = field.replace(/^\$\./, '')
    fieldErrors[key.charAt(0).toUpperCase() + key.slice(1)] = messages[0]
  }

  const message =
    res.status === 429
      ? 'Too many attempts from your connection. Please wait a few minutes and try again.'
      : (data.title ?? `Something went wrong (${res.status}). Please try again later.`)
  return { ok: false, status: res.status, message, fieldErrors }
}

export const submitApplication = (req: ApplicationRequest) => post('/api/applications', req)

export const verifyEmail = (token: string) => post('/api/applications/verify', { token })
