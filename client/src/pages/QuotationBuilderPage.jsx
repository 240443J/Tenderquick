import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Box, Paper, Typography, Button, TextField, IconButton, Checkbox, FormControlLabel,
  CircularProgress, Divider, Chip, Snackbar, Alert, Table, TableBody, TableCell,
  TableHead, TableRow, MenuItem, Tooltip,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlineOutlined'
import AddIcon from '@mui/icons-material/Add'
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf'
import VerifiedIcon from '@mui/icons-material/Verified'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import * as quotationsApi from '../api/quotations'
import { useAuth } from '../context/AuthContext'
import { tokens, monoSx } from '../theme'
import { formatCurrency } from '../utils/format'
import { computeTotals, exportQuotePdf } from '../utils/quote'

function TotalsRow({ label, value, bold }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 0.5 }}>
      <Typography variant="body2" sx={{ fontWeight: bold ? 800 : 400, fontSize: bold ? '1.05rem' : undefined }}>
        {label}
      </Typography>
      <Typography sx={{ ...monoSx, fontWeight: bold ? 800 : 600, fontSize: bold ? '1.05rem' : undefined }}>
        {formatCurrency(value)}
      </Typography>
    </Box>
  )
}

export default function QuotationBuilderPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { user } = useAuth()

  const [quote, setQuote] = useState(null)
  const [confirmed, setConfirmed] = useState(false)
  const [toast, setToast] = useState('')

  const { data, isLoading } = useQuery({
    queryKey: ['quotation', id],
    queryFn: () => quotationsApi.getById(id).then((r) => r.data),
  })

  useEffect(() => {
    if (data) {
      setQuote(data)
      setConfirmed(data.verified)
    }
  }, [data])

  if (isLoading || !quote) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
  }

  const totals = computeTotals(quote)

  const editLine = (lineId, field, value) => {
    setQuote((q) => ({
      ...q,
      lineItems: q.lineItems.map((li) => (li.id === lineId ? { ...li, [field]: value } : li)),
    }))
  }
  const removeLine = (lineId) => {
    setQuote((q) => ({ ...q, lineItems: q.lineItems.filter((li) => li.id !== lineId) }))
  }
  const addLine = () => {
    const nid = Math.max(0, ...quote.lineItems.map((li) => li.id)) + 1
    setQuote((q) => ({
      ...q,
      lineItems: [...q.lineItems, { id: nid, kind: 'equipment', desc: '', qty: 1, unit: 'each', unitPrice: 0 }],
    }))
  }

  const saveDraft = async () => {
    await quotationsApi.update(id, { lineItems: quote.lineItems, markupPct: Number(quote.markupPct), gstPct: Number(quote.gstPct) })
    queryClient.invalidateQueries({ queryKey: ['quotations'] })
    setToast('Quotation saved.')
  }

  const verify = async () => {
    await quotationsApi.update(id, { lineItems: quote.lineItems, markupPct: Number(quote.markupPct), gstPct: Number(quote.gstPct) })
    const res = await quotationsApi.verify(id, user.name)
    setQuote(res.data)
    queryClient.invalidateQueries({ queryKey: ['quotations'] })
    queryClient.invalidateQueries({ queryKey: ['audit'] })
    setToast('Quotation verified — ready to export.')
  }

  const exportPdf = () => {
    const ok = exportQuotePdf(quote)
    if (!ok) setToast('Please allow pop-ups to export the PDF.')
  }

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/quotations')} sx={{ mb: 2, color: tokens.textSecondary }}>
        Back to quotations
      </Button>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 2, mb: 1, flexWrap: 'wrap' }}>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
            <Typography sx={{ ...monoSx, color: tokens.textSecondary }}>{quote.quoteNo}</Typography>
            {quote.verified ? (
              <Chip icon={<VerifiedIcon />} label="Verified" size="small" sx={{ bgcolor: tokens.statusOnTrackBg, color: tokens.statusOnTrack, fontWeight: 700 }} />
            ) : (
              <Chip icon={<AutoAwesomeIcon sx={{ fontSize: '0.9rem !important' }} />} label="AI draft — unverified" size="small" sx={{ bgcolor: tokens.statusSoonBg, color: tokens.statusSoon, fontWeight: 700 }} />
            )}
          </Box>
          <Typography variant="h2">{quote.title}</Typography>
          <Typography variant="body2" sx={{ color: tokens.textSecondary }}>
            {quote.client} · Tender {quote.tenderRef}
          </Typography>
        </Box>
      </Box>

      {!quote.verified && (
        <Alert severity="warning" icon={<AutoAwesomeIcon />} sx={{ mb: 3 }}>
          This quotation was drafted by AI from the tender specs and your inventory prices.
          Review every line, then confirm the human check below before exporting.
        </Alert>
      )}

      <Box sx={{ display: 'flex', gap: 3, alignItems: 'flex-start', flexDirection: { xs: 'column', lg: 'row' } }}>
        {/* Line items */}
        <Paper sx={{ p: { xs: 1.5, md: 2.5 }, borderRadius: 3, flex: 1, width: '100%', minWidth: 0, overflowX: 'auto' }}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': { fontWeight: 700, color: tokens.textSecondary } }}>
                <TableCell>Description</TableCell>
                <TableCell align="right" sx={{ width: 80 }}>Qty</TableCell>
                <TableCell sx={{ width: 90 }}>Unit</TableCell>
                <TableCell align="right" sx={{ width: 120 }}>Unit Price</TableCell>
                <TableCell align="right" sx={{ width: 120 }}>Amount</TableCell>
                <TableCell sx={{ width: 40 }} />
              </TableRow>
            </TableHead>
            <TableBody>
              {quote.lineItems.map((li) => (
                <TableRow key={li.id}>
                  <TableCell>
                    <TextField
                      value={li.desc}
                      onChange={(e) => editLine(li.id, 'desc', e.target.value)}
                      variant="standard"
                      fullWidth
                      InputProps={{ disableUnderline: true }}
                      placeholder="Item description"
                    />
                    <Chip
                      label={li.kind}
                      size="small"
                      sx={{ height: 16, fontSize: '0.6rem', mt: 0.25, bgcolor: li.kind === 'labour' ? tokens.statusDraftBg : tokens.accentIndigoSubtle, color: li.kind === 'labour' ? tokens.statusDraft : tokens.accentIndigo }}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <TextField
                      value={li.qty}
                      onChange={(e) => editLine(li.id, 'qty', e.target.value === '' ? '' : Number(e.target.value))}
                      type="number" variant="standard" sx={{ width: 64 }}
                      inputProps={{ style: { textAlign: 'right' } }}
                      InputProps={{ disableUnderline: true }}
                    />
                  </TableCell>
                  <TableCell>
                    <TextField
                      select value={li.unit} onChange={(e) => editLine(li.id, 'unit', e.target.value)}
                      variant="standard" InputProps={{ disableUnderline: true }} sx={{ width: 76 }}
                    >
                      {['each', 'meter', 'box', 'hour', 'day', 'lot'].map((u) => <MenuItem key={u} value={u}>{u}</MenuItem>)}
                    </TextField>
                  </TableCell>
                  <TableCell align="right">
                    <TextField
                      value={li.unitPrice}
                      onChange={(e) => editLine(li.id, 'unitPrice', e.target.value === '' ? '' : Number(e.target.value))}
                      type="number" variant="standard" sx={{ width: 96 }}
                      inputProps={{ style: { textAlign: 'right' } }}
                      InputProps={{ disableUnderline: true }}
                    />
                  </TableCell>
                  <TableCell align="right" sx={{ ...monoSx }}>
                    {formatCurrency(Number(li.qty || 0) * Number(li.unitPrice || 0))}
                  </TableCell>
                  <TableCell>
                    <IconButton size="small" onClick={() => removeLine(li.id)} sx={{ color: tokens.textMuted }}>
                      <DeleteOutlineIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Button startIcon={<AddIcon />} size="small" onClick={addLine} sx={{ mt: 1 }}>
            Add line item
          </Button>
        </Paper>

        {/* Summary + actions */}
        <Paper sx={{ p: 3, borderRadius: 3, width: { xs: '100%', lg: 320 }, flexShrink: 0, position: { lg: 'sticky' }, top: 88 }}>
          <Typography variant="h4" sx={{ mb: 1.5 }}>Summary</Typography>

          <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5 }}>
            <TextField
              label="Margin %" type="number" size="small" value={quote.markupPct}
              onChange={(e) => setQuote((q) => ({ ...q, markupPct: e.target.value }))}
              sx={{ flex: 1 }}
            />
            <TextField
              label="GST %" type="number" size="small" value={quote.gstPct}
              onChange={(e) => setQuote((q) => ({ ...q, gstPct: e.target.value }))}
              sx={{ flex: 1 }}
            />
          </Box>

          <Divider sx={{ my: 1 }} />
          <TotalsRow label="Subtotal" value={totals.subtotal} />
          <TotalsRow label={`Margin (${quote.markupPct || 0}%)`} value={totals.markup} />
          <TotalsRow label={`GST (${quote.gstPct || 0}%)`} value={totals.gst} />
          <Divider sx={{ my: 1 }} />
          <TotalsRow label="Total" value={totals.total} bold />

          <Divider sx={{ my: 2 }} />

          <FormControlLabel
            control={(
              <Checkbox
                checked={confirmed}
                disabled={quote.verified}
                onChange={(e) => setConfirmed(e.target.checked)}
              />
            )}
            label={<Typography variant="body2" sx={{ fontWeight: 600 }}>I have checked this quotation as a human</Typography>}
            sx={{ alignItems: 'flex-start', mb: 1 }}
          />

          {!quote.verified ? (
            <Button
              fullWidth variant="contained" startIcon={<VerifiedIcon />}
              disabled={!confirmed}
              onClick={verify}
              sx={{ mb: 1 }}
            >
              Verify quotation
            </Button>
          ) : (
            <Typography variant="caption" sx={{ color: tokens.statusOnTrack, display: 'block', mb: 1, fontWeight: 600 }}>
              ✓ Verified by {quote.verifiedBy}
            </Typography>
          )}

          <Tooltip title={quote.verified ? '' : 'Verify the quotation first'}>
            <span>
              <Button
                fullWidth variant="outlined" startIcon={<PictureAsPdfIcon />}
                disabled={!quote.verified}
                onClick={exportPdf}
                sx={{ mb: 1 }}
              >
                Export PDF
              </Button>
            </span>
          </Tooltip>
          <Button fullWidth onClick={saveDraft} sx={{ color: tokens.textSecondary }}>
            Save without verifying
          </Button>
        </Paper>
      </Box>

      <Snackbar
        open={Boolean(toast)} autoHideDuration={3000} onClose={() => setToast('')}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="success" variant="filled" onClose={() => setToast('')}>{toast}</Alert>
      </Snackbar>
    </Box>
  )
}
