import { useState } from 'react'
import { useNavigate, useLocation, Navigate } from 'react-router-dom'
import { useFormik } from 'formik'
import * as Yup from 'yup'
import {
  Box, Paper, TextField, Button, Typography, Alert, Stack,
} from '@mui/material'
import { useAuth } from '../context/AuthContext'
import { tokens } from '../theme'

const schema = Yup.object({
  email: Yup.string().email('Enter a valid email').required('Email is required'),
  password: Yup.string().required('Password is required'),
})

export default function LoginPage() {
  const { user, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [error, setError] = useState('')

  const from = location.state?.from?.pathname || '/'

  const formik = useFormik({
    initialValues: { email: '', password: '' },
    validationSchema: schema,
    onSubmit: async (values, { setSubmitting }) => {
      setError('')
      try {
        await login(values)
        navigate(from, { replace: true })
      } catch (err) {
        setError(err.response?.data?.message || 'Login failed. Please try again.')
      } finally {
        setSubmitting(false)
      }
    },
  })

  if (user) return <Navigate to="/" replace />

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: tokens.navy,
        p: 2,
      }}
    >
      <Paper sx={{ p: 4, width: '100%', maxWidth: 400, borderRadius: 3 }}>
        <Typography variant="h2" sx={{ mb: 0.5 }}>Tenderquick</Typography>
        <Typography variant="body2" sx={{ color: tokens.textSecondary, mb: 3 }}>
          Sign in to your workspace
        </Typography>

        <Box component="form" onSubmit={formik.handleSubmit}>
          <Stack spacing={2}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
              label="Email"
              name="email"
              type="email"
              value={formik.values.email}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.email && Boolean(formik.errors.email)}
              helperText={formik.touched.email && formik.errors.email}
              fullWidth
              autoFocus
            />
            <TextField
              label="Password"
              name="password"
              type="password"
              value={formik.values.password}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.password && Boolean(formik.errors.password)}
              helperText={formik.touched.password && formik.errors.password}
              fullWidth
            />
            <Button
              type="submit"
              variant="contained"
              size="large"
              disabled={formik.isSubmitting}
              sx={{ py: 1.25 }}
            >
              {formik.isSubmitting ? 'Signing in…' : 'Sign In'}
            </Button>
          </Stack>
        </Box>
      </Paper>
    </Box>
  )
}
