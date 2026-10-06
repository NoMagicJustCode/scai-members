import { Link } from 'react-router-dom'
import PageHero from '../components/PageHero'

export default function NotFound() {
  return (
    <PageHero eyebrow="404" title="Page not found">
      <div className="cta-row">
        <Link to="/" className="cta">Back to the start page</Link>
      </div>
    </PageHero>
  )
}
