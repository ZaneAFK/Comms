import { defineStore } from 'pinia'
import type { User, RegisterResponse } from '@/types'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
	const user = ref<User | null>(null)
	const token = ref<string | null>(null)
	// bypassAuth IS DEV ONLY, REMOVE FLAG FOR PRODUCTION
	const isAuthenticated = computed(() => !!token.value || (import.meta.env.DEV && localStorage.getItem('bypassAuth') === 'true'))

	// Initialize from localStorage
	const storedUser = localStorage.getItem('user')
	const storedToken = localStorage.getItem('token')

	if (storedUser) user.value = JSON.parse(storedUser)
	if (storedToken) token.value = storedToken

	async function login(email: string, password: string) {
		const res = await fetch('/api/authentication/login', {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ email, password })
		})

		const body = await res.json().catch(() => ({}))

		if (!res.ok) {
			return { success: false, error: 'Login failed' }
		}

		if (!body.succeeded) {
			return { success: false, error: body.error ?? 'Login failed' }
		}

		token.value = body.token
		user.value = body.user
		localStorage.setItem('token', body.token)
		localStorage.setItem('user', JSON.stringify(body.user))

		return { success: true }
	}

	async function register(username: string, email: string, password: string) {
		const res = await fetch('/api/authentication/register', {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ username, email, password })
		})

		const body = await res.json().catch(() => ({})) as RegisterResponse

		if (!res.ok) {
			return { success: false, error: 'Registration failed' }
		}

		if (!body.succeeded) {
			return { success: false, error: body.error ?? 'Registration failed' }
		}

		return { success: true }
	}

	function logout() {
		user.value = null
		token.value = null
		localStorage.removeItem('user')
		localStorage.removeItem('token')
	}

	return {
		user,
		token,
		isAuthenticated,
		login,
		register,
		logout
	}
})
