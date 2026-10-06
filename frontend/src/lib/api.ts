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
export type MembershipClass = 'Ordinary' | 'Supporting' | 'Honorary'
export type ApplicableClass = Exclude<MembershipClass, 'Honorary'>
export type MemberStatus = 'Applied' | 'Active' | 'Resigned' | 'Excluded' | 'Ended'
export type ApplicationState = 'Pending' | 'Approved' | 'Rejected' | 'Withdrawn'

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

export type ApiResult<T = unknown> =
  | { ok: true; data: T; message: string }
  | { ok: false; status: number; message: string; fieldErrors: FieldErrors }

interface ProblemDetails {
  title?: string
  message?: string
  errors?: Record<string, string[]>
}

/**
 * All calls send the session cookie (credentials) and the custom header the
 * API requires on mutating requests as CSRF protection.
 */
async function request<T>(method: string, path: string, body?: unknown): Promise<ApiResult<T>> {
  let res: Response
  try {
    res = await fetch(`${API_BASE_URL}${path}`, {
      method,
      credentials: 'include',
      headers: {
        'X-SCAI-Request': '1',
        ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
      },
      body: body !== undefined ? JSON.stringify(body) : undefined,
    })
  } catch {
    return { ok: false, status: 0, message: 'Could not reach the server. Please check your connection and try again.', fieldErrors: {} }
  }

  const data = (await res.json().catch(() => ({}))) as ProblemDetails
  if (res.ok) return { ok: true, data: data as T, message: data.message ?? '' }

  const fieldErrors: FieldErrors = {}
  for (const [field, messages] of Object.entries(data.errors ?? {})) {
    // Model-binding errors arrive as "$.field" or "Field"; normalise to the property name.
    const key = field.replace(/^\$\./, '')
    fieldErrors[key.charAt(0).toUpperCase() + key.slice(1)] = messages[0]
  }

  const message =
    res.status === 429
      ? 'Too many attempts from your connection. Please wait a few minutes and try again.'
      : res.status === 401
        ? (data.title ?? 'Please sign in.')
        : (data.title ?? `Something went wrong (${res.status}). Please try again later.`)
  return { ok: false, status: res.status, message, fieldErrors }
}

// --- Public ------------------------------------------------------------

export const submitApplication = (req: ApplicationRequest) => request('POST', '/api/applications', req)

export const verifyEmail = (token: string) => request('POST', '/api/applications/verify', { token })

// --- Session -----------------------------------------------------------

export interface Session {
  id: string
  name: string
  email: string
  isAdmin: boolean
}

export const login = (email: string, password: string) =>
  request<Session>('POST', '/api/auth/login', { email, password })

export const logout = () => request('POST', '/api/auth/logout')

export const fetchSession = () => request<Session>('GET', '/api/auth/me')

// --- Board -------------------------------------------------------------

export interface AdminApplication {
  id: string
  memberType: MemberType
  name: string
  representative: string | null
  dateOfBirth: string | null
  email: string
  postalAddress: string | null
  country: string | null
  requestedClass: MembershipClass
  motivation: string | null
  submittedAt: string
  state: ApplicationState
  decidedAt: string | null
  decidedBy: string | null
}

export interface DecisionResponse {
  message: string
  emailSent: boolean
}

export interface AdminMember {
  id: string
  memberType: MemberType
  name: string
  representative: string | null
  email: string
  membershipClass: MembershipClass
  status: MemberStatus
  joinedAt: string | null
  isAdmin: boolean
}

export interface AdminMemberList {
  members: AdminMember[]
  activeCount: number
  oneTenth: number
}

export interface PlatformSettings {
  joiningFee: number
  annualFeeOrdinary: number
  annualFeeSupporting: number
  currency: string
  statutesVersion: string
  privacyPolicyVersion: string
  updatedAt: string | null
}

export const fetchApplications = (pending: boolean) =>
  request<AdminApplication[]>('GET', `/api/admin/applications?pending=${pending}`)

export const approveApplication = (id: string) =>
  request<DecisionResponse>('POST', `/api/admin/applications/${id}/approve`)

export const rejectApplication = (id: string) =>
  request<DecisionResponse>('POST', `/api/admin/applications/${id}/reject`)

export const fetchMembers = () => request<AdminMemberList>('GET', '/api/admin/members')

export const fetchSettings = () => request<PlatformSettings>('GET', '/api/admin/settings')

export const saveSettings = (s: PlatformSettings) => request<PlatformSettings>('PUT', '/api/admin/settings', s)
