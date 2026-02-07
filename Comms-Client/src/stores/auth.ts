import { defineStore } from 'pinia'
import type { User, LoginSuccessResponse } from '@/types'
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
		const res = await fetch('api/login', {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ email, password })
		})

		const body = await res.json().catch(() => ({}))

		if (!res.ok) {
			const errorMessage = 'error' in body ? body.error : 'Login failed'
			return { success: false, error: errorMessage }
		}

		const { token: retrievedToken, user: retrievedUser } = body as LoginSuccessResponse

		token.value = retrievedToken
		user.value = retrievedUser
		localStorage.setItem('token', retrievedToken)
		localStorage.setItem('user', JSON.stringify(retrievedUser))

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
		logout
	}
})
