// Simulates token-by-token AI streaming for the prototype. Calls onUpdate with
// progressively longer slices of `full`, then resolves when complete.
// Returns a cancel function.
export function streamText(full, onUpdate, { stepMs = 16, chunk = 4, onDone } = {}) {
  let i = 0
  const id = setInterval(() => {
    i = Math.min(i + chunk, full.length)
    onUpdate(full.slice(0, i))
    if (i >= full.length) {
      clearInterval(id)
      onDone?.()
    }
  }, stepMs)
  return () => clearInterval(id)
}
