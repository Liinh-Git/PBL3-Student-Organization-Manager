import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// TODO Phase 3D: Configure proxy if needed for API calls during development
// For now, frontend will call API directly via VITE_API_BASE_URL

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    // TODO Phase 3D: Add proxy configuration if needed
    // proxy: {
    //   '/api': {
    //     target: 'http://localhost:5000',
    //     changeOrigin: true
    //   }
    // }
  }
})
