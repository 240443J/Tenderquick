import {
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
} from '@mui/material'
import { tokens } from '../../theme'

// Standardised table wrapper.
// columns: [{ key, label, align?, render?(row) }]
export default function DataTable({ columns, rows, getRowKey, onRowClick }) {
  return (
    <TableContainer component={Paper} variant="outlined" sx={{ borderColor: tokens.borderLight }}>
      <Table size="small">
        <TableHead>
          <TableRow sx={{ bgcolor: tokens.offWhite }}>
            {columns.map((col) => (
              <TableCell
                key={col.key}
                align={col.align || 'left'}
                sx={{ fontWeight: 700, color: tokens.textSecondary, whiteSpace: 'nowrap' }}
              >
                {col.label}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row) => (
            <TableRow
              key={getRowKey(row)}
              hover
              onClick={onRowClick ? () => onRowClick(row) : undefined}
              sx={{
                cursor: onRowClick ? 'pointer' : 'default',
                '&:hover': { bgcolor: tokens.accentIndigoSubtle },
              }}
            >
              {columns.map((col) => (
                <TableCell key={col.key} align={col.align || 'left'}>
                  {col.render ? col.render(row) : row[col.key]}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  )
}
