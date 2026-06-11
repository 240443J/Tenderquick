import { useState } from 'react'
import { useNavigate, useLocation, Navigate } from 'react-router-dom'
import { useFormik } from 'formik'
import * as Yup from 'yup'
import { Helmet } from 'react-helmet-async'
import {
  Alert, Box, Button, Card, CardContent, Stack, TextField, Typography,
} from '@mui/material'
import { useAuth } from '../context/AuthContext'

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
        setError(err.response?.data?.message || 'Invalid email or password')
      } finally {
        setSubmitting(false)
      }
    },
  })

  if (user) return <Navigate to="/" replace />

  return (
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', bgcolor: 'background.default', p: 2 }}>
      <Helmet><title>Sign in · Tenderquick</title></Helmet>
      <Card sx={{ width: '100%', maxWidth: 420 }} elevation={3}>
        <CardContent sx={{ p: 4 }}>
          <Typography variant="h4" color="primary" sx={{ fontWeight: 800, mb: 0.5 }}>Tenderquick</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>Sign in to continue</Typography>

          <form onSubmit={formik.handleSubmit}>
            <Stack spacing={2}>
              {error && <Alert severity="error">{error}</Alert>}
              <TextField
                name="email" label="Email" fullWidth autoFocus
                value={formik.values.email} onChange={formik.handleChange} onBlur={formik.handleBlur}
                error={formik.touched.email && Boolean(formik.errors.email)}
                helperText={formik.touched.email && formik.errors.email}
              />
              <TextField
                name="password" label="Password" type="password" fullWidth
                value={formik.values.password} onChange={formik.handleChange} onBlur={formik.handleBlur}
                error={formik.touched.password && Boolean(formik.errors.password)}
                helperText={formik.touched.password && formik.errors.password}
              />
              <Button type="submit" variant="contained" size="large" disabled={formik.isSubmitting}>
                {formik.isSubmitting ? 'Signing in…' : 'Sign in'}
              </Button>
            </Stack>
          </form>

          <Alert severity="info" sx={{ mt: 3 }}>
            <strong>Dev logins</strong><br />
            admin@tenderquick.local · Admin#123<br />
            estimator@tenderquick.local · Estimator#123<br />
            viewer@tenderquick.local · Viewer#123
          </Alert>
        </CardContent>
      </Card>
    </Box>
  )
}
