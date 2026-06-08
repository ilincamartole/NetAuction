import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { resolve } from 'path'

export default defineConfig({
    plugins: [react()],
    build: {
        outDir: resolve(__dirname, '../wwwroot/dist'),
        emptyOutDir: true,
        rollupOptions: {
            input: {
                main: resolve(__dirname, 'src/main.jsx'),
                oferte: resolve(__dirname, 'src/oferte.jsx'),
                details: resolve(__dirname, 'src/details.jsx'),
                favorite: resolve(__dirname, 'src/favorite.jsx') 
            },
            output: {
                entryFileNames: '[name].js',
                assetFileNames: 'assets/[name].[ext]'
            }
        }
    }
})