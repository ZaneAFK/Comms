import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
	plugins: [
		vue(),
		vueDevTools(),
		tailwindcss()
	],
	server: {
		proxy: {
			'/api': 'http://localhost:5177',
			'/hubs': {
				target: 'http://localhost:5177',
				ws: true
			}
		}
	},
	test: {
		environment: 'jsdom',
		globals: true
	},
	resolve: {
		alias: {
			'@': fileURLToPath(new URL('./src', import.meta.url))
		}
	},
})
