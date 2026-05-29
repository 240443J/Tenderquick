import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': { target: 'http://localhost:5043', changeOrigin: true },
      '/uploads': { target: 'http://localhost:5043', changeOrigin: true },
    },
  },
})
