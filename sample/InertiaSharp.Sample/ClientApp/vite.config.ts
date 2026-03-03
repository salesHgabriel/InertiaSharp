import path from 'node:path'
import { fileURLToPath, URL } from 'node:url'
import vue from '@vitejs/plugin-vue'
import autoprefixer from 'autoprefixer'
import tailwind from 'tailwindcss'
import { defineConfig } from 'vite'

/**
 * Vite configuration — matches the shadcn-vue Vite installation guide exactly.
 * https://www.shadcn-vue.com/docs/installation/vite
 *
 * DEV:  `npm run dev` (port 5173) — Vite HMR + ASP.NET Core on port 5001
 * PROD: `npm run build` — outputs hashed assets to ../../wwwroot/dist/
 */

const target = process.env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}` :
    process.env.ASPNETCORE_URLS ? process.env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:5001';

console.log(`ASP.NET Core target: ${target}`);

export default defineConfig({
  css: {
    postcss: {
      plugins: [tailwind(), autoprefixer()],
    },
  },
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    strictPort: true,
    cors: { origin: [target] },
    hmr: { host: 'localhost', port: 5173 },
  },
  build: {
    outDir: path.resolve(__dirname, '../wwwroot/dist'),
    emptyOutDir: true,
    manifest: true,
    rollupOptions: {
      input: path.resolve(__dirname, 'src/app.ts'),
      output: {
        // Nome fixo para o arquivo principal JS
        entryFileNames: 'app.js',
        // Chunks mantêm hash para cache busting
        chunkFileNames: 'chunks/[name]-[hash].js',
        // CSS principal com nome fixo
        assetFileNames: (assetInfo) => {
          const fileName = assetInfo.names?.[0] || 'unknown'
          if (fileName.endsWith('.css')) {
            return 'app.css';
          }
          // Outros assets mantêm hash
          return 'assets/[name]-[hash][extname]';
        }
      }
    },
  },
})

