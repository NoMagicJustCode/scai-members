import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import PageHero, { Divider } from '../components/PageHero'
import { fetchPublicConfig, type PublicConfig } from '../lib/api'
import { feesAreSet, formatFee } from '../lib/fees'
import { ORG } from '../lib/org'

export default function Landing() {
  const [config, setConfig] = useState<PublicConfig | null>(null)

  useEffect(() => {
    fetchPublicConfig().then(setConfig).catch(() => setConfig(null))
  }, [])

  const showFees = config !== null && feesAreSet(config)
  const fee = (amount: number) => (config ? formatFee(amount, config.currency) : '')

  return (
    <>
      <PageHero
        eyebrow="Membership"
        title={<>Join <em>Second Circuit</em></>}
        intro="We believe the future is not a given — it must be demanded, designed and defended. Artificial intelligence must stand with individual people: their needs, their growth, their dignity. As a member you help carry that work, and take part in how the association decides and acts."
      >
        <div className="cta-row">
          <Link to="/apply" className="cta cta--primary">Apply for membership</Link>
          <a href={ORG.statutesUrl} className="cta">Read the statutes</a>
        </div>
      </PageHero>

      <Divider />

      <section className="content-section">
        <div className="section-label">Membership</div>
        <div>
          <h2 className="section-heading">Three ways to belong</h2>
          <div className="pillar-cards">
            <article className="pillar-card">
              <h3>Ordinary</h3>
              <p className="pillar-card-sub">
                {showFees ? `${fee(config.annualFeeOrdinary)} per year` : 'Full participation'}
              </p>
              <p className="pillar-card-body">Takes part fully in the work of the association.</p>
            </article>
            <article className="pillar-card">
              <h3>Extraordinary</h3>
              <p className="pillar-card-sub">
                {showFees ? `${fee(config.annualFeeSupporting)} per year` : 'Supporting'}
              </p>
              <p className="pillar-card-body">
                Supports the association primarily through an increased membership fee.
              </p>
            </article>
            <article className="pillar-card">
              <h3>Honorary</h3>
              <p className="pillar-card-sub">By appointment</p>
              <p className="pillar-card-body">
                Appointed for special merit by the General Assembly, on a motion of the
                board. Honorary membership is not applied for.
              </p>
            </article>
          </div>
          <p className="note">
            Membership types per §6 of the statutes.{' '}
            {showFees
              ? config.joiningFee > 0
                ? `A one-time joining fee of ${fee(config.joiningFee)} applies. Fees are set by the General Assembly.`
                : 'Fees are set by the General Assembly.'
              : 'Membership fees are set by the General Assembly and will be published here.'}
          </p>
        </div>
      </section>

      <Divider />

      <section className="content-section">
        <div className="section-label">Joining</div>
        <div className="framework-items">
          <div className="framework-item">
            <h4><span className="framework-num">I.</span>Apply</h4>
            <p>
              Natural persons aged 18 or over, and organisations, who share the values and
              goals of the association may apply (§7(1)).
            </p>
          </div>
          <div className="framework-item">
            <h4><span className="framework-num">II.</span>Confirm your email</h4>
            <p>
              We send you a link. Your application reaches the board only after you open it.
            </p>
          </div>
          <div className="framework-item">
            <h4><span className="framework-num">III.</span>The board decides</h4>
            <p>
              The board decides on admission and may decline an application without giving
              reasons (§7(2)). You will hear from us by email either way.
            </p>
          </div>
          <div className="cta-row cta-row--start" style={{ marginTop: 0 }}>
            <Link to="/apply" className="cta cta--primary">Apply for membership</Link>
          </div>
        </div>
      </section>

      <Divider />

      <section className="content-section">
        <div className="section-label">Your data</div>
        <div className="mission-content">
          <p>
            We ask only for what the statutes require (§20): your name, date of birth,
            contact details and what fee administration needs. No trackers, no analytics,
            no data outside the EU.
          </p>
          <p>
            Read the <Link to="/privacy">privacy policy</Link>.
          </p>
        </div>
      </section>
    </>
  )
}
