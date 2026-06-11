// Mimics an axios response so api/*.js can return `{ data }` exactly like the
// real backend will. Adds a small latency so loading states are visible in the demo.
export function respond(data, ms = 320) {
  const clone = typeof structuredClone === 'function'
    ? structuredClone(data)
    : JSON.parse(JSON.stringify(data))
  return new Promise((resolve) => setTimeout(() => resolve({ data: clone }), ms))
}
