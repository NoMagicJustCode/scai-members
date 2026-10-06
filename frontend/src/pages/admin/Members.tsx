import { useEffect, useState } from 'react'
import { fetchMembers, type AdminMemberList } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { className, formatDate } from '../../lib/format'

export default function Members() {
  const { expire } = useAuth()
  const [list, setList] = useState<AdminMemberList | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchMembers().then((r) => {
      if (r.ok) setList(r.data)
      else if (r.status === 401) expire()
      else setError(r.message)
    })
  }, [expire])

  return (
    <section>
      <div className="admin-toolbar">
        <h1 className="admin-title">Members</h1>
      </div>

      {error && <div className="notice notice--error"><p>{error}</p></div>}
      {!list && !error && <p className="admin-muted">Loading…</p>}

      {list && (
        <>
          <div className="stat-row">
            <div className="stat">
              <div className="stat__value">{list.activeCount}</div>
              <div className="stat__label">Active members</div>
            </div>
            <div className="stat">
              <div className="stat__value">{list.oneTenth}</div>
              <div className="stat__label">One tenth — threshold of §9(3) and §11(2)</div>
            </div>
          </div>

          <div className="table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Class</th>
                  <th>Status</th>
                  <th>Joined</th>
                </tr>
              </thead>
              <tbody>
                {list.members.map((m) => (
                  <tr key={m.id}>
                    <td>
                      {m.name}
                      {m.isAdmin && <span className="tag">Board</span>}
                      {m.memberType === 'Organisation' && (
                        <div className="admin-muted">represented by {m.representative}</div>
                      )}
                    </td>
                    <td>{m.email}</td>
                    <td>{className(m.membershipClass)}</td>
                    <td>{m.status}</td>
                    <td>{formatDate(m.joinedAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <p className="admin-muted small-print">
            Payments, reminders, ending memberships and data export follow in a later phase.
          </p>
        </>
      )}
    </section>
  )
}
