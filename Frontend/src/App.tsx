import { useState } from 'react'
import './App.css'

const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

export default function App() {
  const [email, setEmail] = useState('demo@hirehub.local')
  const [password, setPassword] = useState('Password12')
  const [role, setRole] = useState('Candidate')
  const [message, setMessage] = useState<string | null>(null)

  async function register() {
    setMessage(null)
    const res = await fetch(`${apiBase}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, role }),
    })
    const body = await res.json().catch(() => ({}))
    setMessage(res.ok ? 'Registered. Tokens returned (see browser console).' : JSON.stringify(body))
    if (res.ok) console.log(body)
  }

  return (
    <main className="hirehub">
      <h1>HireHub</h1>
      <p className="tagline">Microservices recruitment platform (dev UI)</p>
      <p className="hint">API base: {apiBase}</p>
      <div className="card">
        <label>
          Email
          <input value={email} onChange={(e) => setEmail(e.target.value)} autoComplete="email" />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="new-password"
          />
        </label>
        <label>
          Role
          <select value={role} onChange={(e) => setRole(e.target.value)}>
            <option>Candidate</option>
            <option>Recruiter</option>
            <option>Admin</option>
          </select>
        </label>
        <button type="button" onClick={() => void register()}>
          Register via API Gateway
        </button>
        {message && <pre className="result">{message}</pre>}
      </div>
    </main>
  )
}
