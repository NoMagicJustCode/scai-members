// Facts about the association, in one place. Shown on the landing page,
// in the footer, the imprint and the privacy policy.

export const ORG = {
  name: 'Second Circuit – Verein für digitale Gedankenfreiheit',
  nameEnglish: 'Second Circuit – Association for Digital Freedom of Thought',
  shortName: 'Second Circuit',
  alsoKnownAs: 'Second Circuit AI',
  zvr: '1684464197',
  seat: 'Vienna, Austria',
  established: '24 August 2026',
  /** Zustellanschrift from the association register (ZVR) — required in the Impressum (§ 5 ECG). */
  postalAddress: 'c/o Christopher Kamper, Hohlweggasse 42/6, 1030 Vienna, Austria',
  contactEmail: 'ann@scai.world',
  boardEmails: ['ann@scai.world', 'chris@scai.world'],
  website: 'https://scai.world',
  statutesUrl: 'https://scai.world/statutes/',
  statutesAdopted: '4 June 2026',
} as const
