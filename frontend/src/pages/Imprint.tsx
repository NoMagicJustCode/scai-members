import PageHero, { Divider } from '../components/PageHero'
import { ORG } from '../lib/org'

/** Impressum per § 5 ECG and § 25 MedienG; structure follows scai.world/impressum. */
export default function Imprint() {
  return (
    <>
      <PageHero
        eyebrow="Legal"
        title="Impressum"
        intro="Information in accordance with § 5 ECG (Austrian E-Commerce Act) and § 25 MedienG."
      />
      <Divider />

      <section className="legal-section">
        <h2>Responsible party</h2>
        <p>
          {ORG.name}
          <br />
          {ORG.nameEnglish} (short: {ORG.shortName}, {ORG.alsoKnownAs})
          <br />
          Austrian association (Verein) · ZVR {ORG.zvr} · established {ORG.established}
          <br />
          Registered seat: {ORG.seat}
        </p>
        <p>
          Address for correspondence:
          <br />
          {ORG.postalAddress}
        </p>

        <h2>Contact</h2>
        <p>
          {ORG.boardEmails.map((email, i) => (
            <span key={email}>
              {i > 0 && <br />}
              <a href={`mailto:${email}`}>{email}</a>
            </span>
          ))}
        </p>

        <h2>Purpose</h2>
        <p>
          The purpose of the association is set out in its{' '}
          <a href={ORG.statutesUrl}>statutes</a>, adopted at the founding assembly in
          Vienna on {ORG.statutesAdopted}.
        </p>

        <h2>Dispute resolution</h2>
        <p>
          The European Commission provides a platform for online dispute resolution (OS):{' '}
          <a href="https://ec.europa.eu/consumers/odr/" target="_blank" rel="noopener noreferrer">
            https://ec.europa.eu/consumers/odr/
          </a>
        </p>
        <p>
          We are not willing or obliged to participate in dispute resolution proceedings
          before a consumer arbitration board.
        </p>
      </section>
    </>
  )
}
