import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Forward /api calls to the ASP.NET Core backend so the browser sees one origin (no CORS setup needed).
      '/api': 'http://localhost:5022',
    },
  },
});
