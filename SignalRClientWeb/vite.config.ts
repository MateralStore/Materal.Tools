import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'fs'
import archiver from 'archiver'

function cleanDistPlugin() {
  return {
    name: 'clean-dist',
    buildStart() {
      const distDir = 'dist'
      if (fs.existsSync(distDir)) {
        fs.rmSync(distDir, { recursive: true, force: true })
        console.log('已清空 dist 文件夹')
      }
    },
  }
}

function zipBuildPlugin() {
  return {
    name: 'zip-build',
    closeBundle() {
      const now = new Date()
      const timestamp = now.toISOString().slice(0, 19).replace(/[-T:]/g, '')
      const outDir = 'dist/SignalRClientWeb'
      const zipPath = `dist/SignalRClientWeb${timestamp}.zip`

      return new Promise<void>((resolve) => {
        const output = fs.createWriteStream(zipPath)
        const archive = archiver('zip', { zlib: { level: 9 } })

        output.on('close', () => {
          console.log(`已压缩为: ${zipPath}`)
          resolve()
        })

        archive.on('error', (err) => {
          throw err
        })

        archive.pipe(output)
        archive.directory(outDir, 'SignalRClientWeb')
        archive.finalize()
      })
    },
  }
}

export default defineConfig({
  plugins: [react(), cleanDistPlugin(), zipBuildPlugin()],
  base: '/SignalRClientWeb/',
  build: {
    outDir: 'dist/SignalRClientWeb',
  },
})
