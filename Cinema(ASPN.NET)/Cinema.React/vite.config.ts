import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Dev server config
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: [{ find: '@', replacement: '/src' }],
  },
  server: {
    proxy: {
      '/api': {
        target: "https://localhost:7266", // http://localhost:5257
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, '')
      },
      '/signalr': {
        target: 'https://localhost:7266', // ws://localhost:5257
        ws: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/signalr/, '')
      },
    },
  },
})

