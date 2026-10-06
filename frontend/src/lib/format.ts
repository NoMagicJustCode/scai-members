import type { MembershipClass } from './api'

export const formatDate = (iso: string | null) =>
  iso ? new Date(iso).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' }) : '—'

/** UI names follow the statutes (§6): "Supporting" in code is "Extraordinary" in the statutes. */
export const className = (c: MembershipClass) =>
  c === 'Supporting' ? 'Extraordinary' : c
