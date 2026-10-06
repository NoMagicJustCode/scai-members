import PageHero, { Divider } from '../components/PageHero'
import { ORG } from '../lib/org'

/**
 * Privacy policy for members.scai.world. Its version must match
 * PlatformConfig.privacyPolicyVersion — applicants consent to that version.
 */
export default function Privacy() {
  return (
    <>
      <PageHero
        eyebrow="Legal"
        title="Privacy Policy"
        intro="How the membership platform of Second Circuit handles your personal data."
        meta="Version 1.0 · October 2026 · members.scai.world"
      />
      <Divider />

      <section className="legal-section">
        <h2>Who is responsible</h2>
        <p>
          {ORG.name}
          <br />
          {ORG.postalAddress}
          <br />
          ZVR {ORG.zvr}
          <br />
          <a href={`mailto:${ORG.contactEmail}`}>{ORG.contactEmail}</a>
        </p>
        <p>
          The association processes personal data only in accordance with the GDPR and
          Austrian law, only as far as needed for its purposes, and never passes it on for
          commercial purposes (§20 of the statutes).
        </p>

        <h2>What we process, and why</h2>
        <h3>Membership applications</h3>
        <p>
          Your name, date of birth (persons) or the name of your representative
          (organisations), your email address, optionally your postal address, country and
          a short motivation, the membership class you apply for, and your declarations
          with the version of the statutes and privacy policy you agreed to and the time
          you did so. We use this to let the board decide on your admission (§7 of the
          statutes). Legal basis: Art. 6(1)(b) GDPR — steps taken at your request before
          membership.
        </p>

        <h3>Membership</h3>
        <p>
          Once admitted, the same data forms your member record, together with fee
          administration: payments received and payment reminders sent. Legal basis:
          Art. 6(1)(b) GDPR (membership) and, for accounting records, Art. 6(1)(c) GDPR
          (statutory retention duties).
        </p>

        <h3>Email</h3>
        <p>
          We send only transactional email: the confirmation link for your application, the
          board's decision, invitations to the General Assembly (§11(3) of the statutes) and
          fee reminders. No newsletters, no tracking in emails.
        </p>

        <h3>Abuse protection</h3>
        <p>
          To limit automated misuse of the application form, your IP address is held in
          memory for up to 15 minutes and is not stored. Our web server keeps standard
          access logs (IP address, time, requested page, browser type) for security, for no
          longer than 14 days. Legal basis: Art. 6(1)(f) GDPR — our legitimate interest in
          operating the platform securely.
        </p>

        <h2>How long we keep it</h2>
        <ul>
          <li>Applications whose email address is not confirmed within 48 hours are deleted automatically.</li>
          <li>Declined applications are deleted within 30 days of the board's decision.</li>
          <li>
            When a membership ends, the member record is deleted. Payment records are kept for
            seven years as required by Austrian accounting law (§132 BAO).
          </li>
        </ul>

        <h2>Who receives it</h2>
        <p>
          Only the board of the association. The platform runs on servers in the European
          Union, operated by a hosting provider acting on our behalf under a data processing
          agreement. Email is sent through a provider in the European Union. No data is
          transferred to countries outside the EU/EEA.
        </p>

        <h2>No cookies, no tracking</h2>
        <p>
          This site sets no cookies, uses no analytics and loads nothing from third parties —
          fonts included, which are served from our own server.
        </p>

        <h2>Your rights</h2>
        <p>
          You have the right to access your data (Art. 15 GDPR), to have it corrected (Art.
          16) or deleted (Art. 17), to restrict its processing (Art. 18), to receive it in a
          portable format (Art. 20) and to object to processing (Art. 21). Write to{' '}
          <a href={`mailto:${ORG.contactEmail}`}>{ORG.contactEmail}</a>.
        </p>
        <p>
          You may also lodge a complaint with the Austrian Data Protection Authority
          (Österreichische Datenschutzbehörde), Barichgasse 40–42, 1030 Vienna,{' '}
          <a href="https://www.dsb.gv.at">www.dsb.gv.at</a>.
        </p>
      </section>
    </>
  )
}
