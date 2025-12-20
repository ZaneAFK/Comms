import { defineStore } from 'pinia'
import type { User } from '@/types'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
	const user = ref<User | null>(null)
	const token = ref<string | null>(null)
	const isAuthenticated = computed(() => !!token.value)

	// Initialize from localStorage
	const storedUser = localStorage.getItem('user')
	const storedToken = localStorage.getItem('token')

	if (storedUser) user.value = JSON.parse(storedUser)
	if (storedToken) token.value = storedToken

	async function login(userData: User, authToken: string) {
		// Login logic here (API CALLS ETC)
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
