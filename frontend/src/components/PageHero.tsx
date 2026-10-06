import type { ReactNode } from 'react'

/** Centered sub-page header, as on scai.world (page-hero + eyebrow). */
export default function PageHero(props: {
  eyebrow: string
  title: ReactNode
  intro?: ReactNode
  meta?: ReactNode
  children?: ReactNode
}) {
  return (
    <header className="page-hero">
      <div className="eyebrow">{props.eyebrow}</div>
      <h1>{props.title}</h1>
      <div className="hero-line" />
      {props.intro && <p className="page-intro">{props.intro}</p>}
      {props.meta && <p className="page-meta">{props.meta}</p>}
      {props.children}
    </header>
  )
}

export function Divider() {
  return (
    <div className="section-divider">
      <hr />
    </div>
  )
}
