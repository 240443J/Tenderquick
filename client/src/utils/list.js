// Query data is only trustworthy once it has resolved successfully. A failed request can
// leave `data` as undefined or an error payload, and calling .map on that white-screens the
// whole page — so every list render goes through this.
export function asArray(value) {
  return Array.isArray(value) ? value : []
}
