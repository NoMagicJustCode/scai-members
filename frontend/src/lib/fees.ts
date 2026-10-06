import type { PublicConfig } from './api'

/** True once the General Assembly's fees have been entered by the board. */
export const feesAreSet = (cfg: PublicConfig) =>
  cfg.joiningFee > 0 || cfg.annualFeeOrdinary > 0 || cfg.annualFeeSupporting > 0

export const formatFee = (amount: number, currency: string) =>
  new Intl.NumberFormat('en-GB', {
    style: 'currency',
    currency,
    minimumFractionDigits: Number.isInteger(amount) ? 0 : 2,
  }).format(amount)
